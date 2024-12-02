using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JasperFx.CodeGeneration;
using JasperFx.Core;
using JasperFx.Core.Reflection;
using Lamar.IoC;
using Lamar.IoC.Enumerables;
using Lamar.IoC.Instances;
using Lamar.IoC.Lazy;
using Lamar.IoC.Resolvers;
using Lamar.IoC.Setters;
using Lamar.Scanning.Conventions;
using Lamar.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar;

public class ServiceGraph : IDisposable, IAsyncDisposable
{
    private readonly Stack<Instance> _chain = new();
    private readonly object _familyLock = new();

    private readonly IList<Type> _lookingFor = new List<Type>();
    private readonly ServiceRegistry _services;
    private Assembly[] _allAssemblies;
    private ImHashMap<Type, Func<Scope, object>> _byType = ImHashMap<Type, Func<Scope, object>>.Empty;

    private ImHashMap<Type, ServiceFamily> _families = ImHashMap<Type, ServiceFamily>.Empty;

    private bool _inPlanning;


    private ServiceGraph(IServiceCollection services, Scope rootScope, AssemblyScanner[] scanners)
    {
        _services = services as ServiceRegistry ?? new ServiceRegistry(services);
        Scanners = scanners;
        RootScope = rootScope;
        organize(_services);
    }

    public ServiceGraph(IServiceCollection services, Scope rootScope)
    {
        var (registry, scanners) = ScanningExploder.ExplodeSynchronously(services);

        Scanners = scanners;

        _services = registry;

        RootScope = rootScope;

        organize(_services);
    }

    public Scope RootScope { get; }

    private ISetterPolicy[] setterPolicies { get; set; }

    public IDecoratorPolicy[] DecoratorPolicies { get; private set; } = new IDecoratorPolicy[0];

    public IInstancePolicy[] InstancePolicies { get; set; }

    public IFamilyPolicy[] FamilyPolicies { get; private set; }

    public IServiceCollection Services => _services;

    public IReadOnlyDictionary<Type, ServiceFamily> Families => _families.ToDictionary();

    internal AssemblyScanner[] Scanners { get; private set; } = new AssemblyScanner[0];

    public async ValueTask DisposeAsync()
    {
        foreach (var instance in AllInstances().OfType<ObjectInstance>())
        {
            if (instance.Service is IAsyncDisposable a)
            {
                try
                {
                    await a.DisposeAsync();
                }
                catch (Exception)
                {
                }
            }
            else if (instance.Service is IDisposable d)
            {
                d.SafeDispose();
            }
        }
    }

    public void Dispose()
    {
        foreach (var instance in AllInstances().OfType<ObjectInstance>())
        {
            if (instance.Service is IDisposable d)
            {
                d.SafeDispose();
            }
            else if (instance.Service is IAsyncDisposable a)
            {
                try
                {
                    a.DisposeAsync().GetAwaiter().GetResult();
                }
                catch (Exception)
                {
                }
            }
        }
    }


    public static async Task<ServiceGraph> BuildAsync(IServiceCollection services, Scope rootScope)
    {
        var (registry, scanners) = await ScanningExploder.Explode(services);

        return new ServiceGraph(registry, rootScope, scanners);
    }

    private void organize(ServiceRegistry services)
    {
        DecoratorPolicies = services.FindAndRemovePolicies<IDecoratorPolicy>();

        FamilyPolicies = services.FindAndRemovePolicies<IFamilyPolicy>()
            .Concat(new IFamilyPolicy[]
            {
                new EnumerablePolicy(),
                new FuncOrLazyPolicy(),
                new CloseGenericFamilyPolicy(),
                new ConcreteFamilyPolicy(),
                new EmptyFamilyPolicy()
            })
            .ToArray();


        setterPolicies = services.FindAndRemovePolicies<ISetterPolicy>();
        InstancePolicies = services.FindAndRemovePolicies<IInstancePolicy>();

        var policies = services.FindAndRemovePolicies<IRegistrationPolicy>();
        foreach (var policy in policies) policy.Apply(services);


        services.Add(new ScopeInstance<Scope>());
        services.Add(new ScopeInstance<IServiceProvider>());
        services.Add(new ScopeInstance<IServiceContext>());

        services.Add(new ScopeInstance<IContainer>());
        ;
        services.Add(new RootScopeInstance<IServiceScopeFactory>());
        ;
    }

    internal bool ShouldBeSet(PropertyInfo property)
    {
        return property.HasAttribute<SetterPropertyAttribute>() || setterPolicies.Any(x => x.Matches(property));
    }

    internal void Inject(Type serviceType, object @object)
    {
        _byType = _byType.AddOrUpdate(serviceType, s => @object);
    }


    public void Initialize()
    {
        organizeIntoFamilies(_services);
        buildOutMissingResolvers();
        rebuildReferencedAssemblyArray();
    }


    private void rebuildReferencedAssemblyArray()
    {
        _allAssemblies = AllInstances().SelectMany(x => x.ReferencedAssemblies())
            .Distinct().ToArray();
    }


    private void buildOutMissingResolvers()
    {
        if (_inPlanning)
        {
            return;
        }

        _inPlanning = true;

        try
        {
            planResolutionStrategies();
        }
        finally
        {
            _inPlanning = false;
        }
    }


    internal GeneratedAssembly ToGeneratedAssembly(string @namespace = null)
    {
        // TODO -- will need to get at the GenerationRules from somewhere
        var generatedAssembly = new GeneratedAssembly(new GenerationRules(@namespace ?? "Lamar.Generated"));

        generatedAssembly.Rules.Assemblies.Fill(_allAssemblies);

        return generatedAssembly;
    }

    private void resetInstancePlanning()
    {
        foreach (var instance in AllInstances()) instance.Reset();
    }

    private void planResolutionStrategies()
    {
        while (AllInstances().Where(x => !x.ServiceType.IsOpenGeneric()).Any(x => !x.HasPlanned))
        {
            foreach (var instance in AllInstances().Where(x => !x.HasPlanned).ToArray()) instance.CreatePlan(this);
        }
    }

    internal Instance FindInstance(ParameterInfo parameter)
    {
        if (parameter.HasAttribute<NamedAttribute>())
        {
            var att = parameter.GetAttribute<NamedAttribute>();
            if (att.TypeName.IsNotEmpty())
            {
                var family = _families.Enumerate().ToArray()
                    .FirstOrDefault(x => x.Value.FullNameInCode == att.TypeName);
                return family?.Value.InstanceFor(att.Name);
            }

            return FindInstance(parameter.ParameterType, att.Name);
        }

        return FindDefault(parameter.ParameterType);
    }

    private void organizeIntoFamilies(IServiceCollection services)
    {
        var serviceFamilies = services
            .Where(x => !x.ServiceType.HasAttribute<JasperFxIgnoreAttribute>())
            .GroupBy(x => x.ServiceType)
            .Select(group => buildFamilyForInstanceGroup(services, group));

        foreach (var family in serviceFamilies) _families = _families.AddOrUpdate(family.ServiceType, family);
    }

    private ServiceFamily buildFamilyForInstanceGroup(IServiceCollection services,
        IGrouping<Type, ServiceDescriptor> group)
    {
        if (group.Key.IsGenericType && !group.Key.IsOpenGeneric())
        {
            return buildClosedGenericType(group.Key, services);
        }

        var instances = group
            .Select(Instance.For)
            .ToArray();

        return new ServiceFamily(group.Key, DecoratorPolicies, instances);
    }

    private ServiceFamily buildClosedGenericType(Type serviceType, IServiceCollection services)
    {
        var closed = services.Where(x => x.ServiceType == serviceType && isKeyedServiceSupported(x))
            .Select(Instance.For);

        var templated = services
            .Where(x => x.ServiceType.IsOpenGeneric() && serviceType.Closes(x.ServiceType))
            .Select(Instance.For)
            .Select(instance =>
            {
                var arguments = serviceType.GetGenericArguments();

                try
                {
                    return instance.CloseType(serviceType, arguments);
                }
                catch (Exception)
                {
                    return null;
                }
            })
            .Where(x => x != null);


        var instances = templated.Concat(closed).ToArray();

        return new ServiceFamily(serviceType, DecoratorPolicies, instances);
    }

    private static bool isKeyedServiceSupported(ServiceDescriptor serviceDescriptor)
    {
        #if NET8_0_OR_GREATER
         return serviceDescriptor.IsKeyedService
                    ? !serviceDescriptor.KeyedImplementationType.IsOpenGeneric()
                    : !serviceDescriptor.ImplementationType.IsOpenGeneric();
        #endif

        return !serviceDescriptor.ImplementationType.IsOpenGeneric();
    }

    public IEnumerable<Instance> AllInstances()
    {
        var serviceFamilies = _families.Enumerate().Select(x => x.Value).Where(x => x != null).ToArray();
        return serviceFamilies.SelectMany(x => x.All).ToArray();
    }

    public bool HasFamily(Type serviceType)
    {
        return _families.Contains(serviceType);
    }

    public Instance FindInstance(Type serviceType, string name)
    {
        return ResolveFamily(serviceType).InstanceFor(name);
    }

    public ServiceFamily ResolveFamily(Type serviceType)
    {
        if (_families.TryFind(serviceType, out var family))
        {
            return family;
        }

        lock (_familyLock)
        {
            if (_families.TryFind(serviceType, out family))
            {
                return family;
            }

            return addMissingFamily(serviceType);
        }
    }

    private ServiceFamily addMissingFamily(Type serviceType)
    {
        var family = TryToCreateMissingFamily(serviceType);

        _families = _families.AddOrUpdate(serviceType, family);

        if (!_inPlanning)
        {
            buildOutMissingResolvers();

            if (family != null)
            {
                rebuildReferencedAssemblyArray();
            }
        }

        return family;
    }

    public Func<Scope, object> FindResolver(Type serviceType)
    {
        if (_byType.TryFind(serviceType, out var resolver))
        {
            return resolver;
        }

        lock (_familyLock)
        {
            if (_byType.TryFind(serviceType, out resolver))
            {
                return resolver;
            }

            var family = ResolveFamily(serviceType);

            var instance = family.Default;
            if (instance == null)
            {
                resolver = null;
            }
            else if (instance.Lifetime == ServiceLifetime.Singleton)
            {
                var inner = instance.ToResolver(RootScope);
                resolver = s =>
                {
                    var value = inner(s);
                    Inject(serviceType, value);

                    return value;
                };
            }
            else
            {
                resolver = instance.ToResolver(RootScope);
            }

            _byType = _byType.AddOrUpdate(serviceType, resolver);

            return resolver;
        }
    }

    public Instance FindDefault(Type serviceType)
    {
        if (serviceType.ShouldIgnore())
        {
            return null;
        }

        return ResolveFamily(serviceType)?.Default;
    }

    public Instance[] FindAll(Type serviceType)
    {
        return ResolveFamily(serviceType)?.All ?? new Instance[0];
    }

    public bool CouldBuild(Type concreteType, out string message)
    {
        var constructorInstance = new ConstructorInstance(concreteType, concreteType, ServiceLifetime.Transient);
        foreach (var policy in InstancePolicies) policy.Apply(constructorInstance);

        var ctor = constructorInstance.DetermineConstructor(this, out message);


        return ctor != null && message.IsEmpty();
    }

    internal void StartingToPlan(Instance instance)
    {
        if (_chain.Contains(instance))
        {
            throw new InvalidOperationException("Bi-directional dependencies detected:" + Environment.NewLine +
                                                _chain.Select(x => x.ToString()).Join(Environment.NewLine));
        }

        _chain.Push(instance);
    }

    internal void FinishedPlanning()
    {
        _chain.Pop();
    }

    public static ServiceGraph Empty()
    {
        return Scope.Empty().ServiceGraph;
    }

    public static ServiceGraph For(Action<ServiceRegistry> configure)
    {
        var registry = new ServiceRegistry();
        configure(registry);

        return new Scope(registry).ServiceGraph;
    }


    public ServiceFamily TryToCreateMissingFamily(Type serviceType)
    {
        if (_lookingFor.Contains(serviceType))
        {
            throw new InvalidOperationException(
                $"Detected some kind of bi-directional dependency while trying to discover and plan a missing service registration. Examining types: {_lookingFor.Select(x => x.FullNameInCode()).Join(", ")}");
        }

        _lookingFor.Add(serviceType);

        if (serviceType.ShouldIgnore())
        {
            return new ServiceFamily(serviceType, DecoratorPolicies);
        }

        var found = FamilyPolicies.FirstValue(x => x.Build(serviceType, this));

        _lookingFor.Remove(serviceType);

        return found;
    }

    internal ServiceFamily TryToCreateMissingFamilyWithNetCoreRules(Type serviceType)
    {
        if (_lookingFor.Contains(serviceType))
        {
            throw new InvalidOperationException(
                $"Detected some kind of bi-directional dependency while trying to discover and plan a missing service registration. Examining types: {_lookingFor.Select(x => x.FullNameInCode()).Join(", ")}");
        }

        _lookingFor.Add(serviceType);

        if (serviceType.ShouldIgnore())
        {
            return new ServiceFamily(serviceType, DecoratorPolicies);
        }

        Type serviceTypeToLookFor = getServiceTypeThatTakesCollectionsIntoAccount(serviceType);

        var found = FamilyPolicies.Where(x => x is not ConcreteFamilyPolicy)
            .FirstValue(x => x.Build(serviceTypeToLookFor, this));

        _lookingFor.Remove(serviceType);

        return found;
    }

    private Type getServiceTypeThatTakesCollectionsIntoAccount(Type serviceType)
    {
        if (!typeof(IEnumerable).IsAssignableFrom(serviceType) || serviceType.GetGenericArguments().Length != 1 || serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>)) 
            return serviceType;

        Type type = serviceType.GetGenericArguments().Single();

        bool isTypeRegistered = _services.Any(descriptor => descriptor.ServiceType == type);

        return isTypeRegistered ? serviceType : type;
    }
    
    internal void ClearPlanning()
    {
        _chain.Clear();
    }

    public bool CouldResolve(Type type)
    {
        try
        {
            return FindDefault(type) != null;
        }
        catch (Exception)
        {
            return false;
        }
    }


    public void AppendServices(IServiceCollection services)
    {
        lock (_familyLock)
        {
            var (registry, scanners) = ScanningExploder.ExplodeSynchronously(services);

            Scanners = Scanners.Union(scanners).ToArray();

            var groups = registry
                .Where(x => !x.ServiceType.HasAttribute<JasperFxIgnoreAttribute>())
                .GroupBy(x => x.ServiceType);

            foreach (var group in groups)
            {
                if (_families.TryFind(group.Key, out var family))
                {
                    if (family.Append(group, DecoratorPolicies) == AppendState.NewDefault)
                    {
                        _byType = _byType.Remove(group.Key);
                    }
                }
                else
                {
                    family = buildFamilyForInstanceGroup(services, group);
                    _families = _families.AddOrUpdate(group.Key, family);
                }
            }


            resetInstancePlanning();

            buildOutMissingResolvers();

            rebuildReferencedAssemblyArray();
        }
    }

    public bool CanBeServiceByNetCoreRules(Type serviceType)
    {
        if (_families.TryFind(serviceType, out var family))
        {
            return family.Default != null;
        }

        lock (_familyLock)
        {
            if (_families.TryFind(serviceType, out family))
            {
                return family.Default != null;
            }

            family = TryToCreateMissingFamilyWithNetCoreRules(serviceType);
            _families = _families.AddOrUpdate(serviceType, family);

            if (!_inPlanning)
            {
                buildOutMissingResolvers();

                if (family != null)
                {
                    rebuildReferencedAssemblyArray();
                }
            }
        }

        return family.Default != null;
    }
}