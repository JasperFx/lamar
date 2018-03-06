using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Lamar.Codegen;
using Lamar.Compilation;
using Lamar.IoC;
using Lamar.IoC.Enumerables;
using Lamar.IoC.Instances;
using Lamar.IoC.Lazy;
using Lamar.IoC.Resolvers;
using Lamar.Scanning.Conventions;
using Lamar.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar
{
    public class ServiceGraph : IDisposable, IModel
    {
        private readonly Scope _rootScope;
        private readonly object _familyLock = new object();
        


        private readonly Dictionary<Type, ServiceFamily> _families = new Dictionary<Type, ServiceFamily>();
        private ImHashMap<Type, Func<Scope, object>> _byType = ImHashMap<Type, Func<Scope, object>>.Empty;


        public ServiceGraph(IServiceCollection services, Scope rootScope)
        {
            
            
            Services = services;


            // This should blow up pretty fast if it's no good
            applyScanners(services).Wait(TimeSpan.FromSeconds(2));

            _rootScope = rootScope;


            FamilyPolicies = services
                .Where(x => x.ServiceType == typeof(IFamilyPolicy))
                .Select(x => x.ImplementationInstance.As<IFamilyPolicy>())
                .Concat(new IFamilyPolicy[]
                {
                    new EnumerablePolicy(),
                    new FuncOrLazyPolicy(),
                    new CloseGenericFamilyPolicy(),
                    new ConcreteFamilyPolicy(),
                    new EmptyFamilyPolicy()
                })
                .ToArray();

            InstancePolicies = services.Where(x => x.ServiceType == typeof(IInstancePolicy) && x.ImplementationInstance is IInstancePolicy)
                .Select(x => x.ImplementationInstance.As<IInstancePolicy>()).ToArray();


            services.RemoveAll(x => x.ServiceType == typeof(IFamilyPolicy));
            services.RemoveAll(x => x.ServiceType == typeof(IInstancePolicy));

            addScopeResolver<Scope>(services);
            addScopeResolver<IServiceProvider>(services);
            addScopeResolver<IContainer>(services);
            addScopeResolver<IServiceScopeFactory>(services);

        }

        internal void Inject(Type serviceType, object @object)
        {
            _byType = _byType.AddOrUpdate(serviceType, s => @object);
        }

        public IInstancePolicy[] InstancePolicies { get; set; }

        private async Task applyScanners(IServiceCollection services)
        {
            _scanners = services.Select(x => x.ImplementationInstance).OfType<AssemblyScanner>().ToArray();
            services.RemoveAll(x => x.ServiceType == typeof(AssemblyScanner));

            foreach (var scanner in _scanners)
            {
                await scanner.ApplyRegistrations(services);
            }

        }

        public IFamilyPolicy[] FamilyPolicies { get; }

        private void addScopeResolver<T>(IServiceCollection services)
        {
            var instance = new ScopeInstance<T>();
            services.Add(instance);
        }

        public void Initialize(PerfTimer timer = null)
        {
            timer = timer ?? new PerfTimer();

            timer.Record("Organize Into Families", () =>
            {
                organizeIntoFamilies(Services);
            });

            timer.Record("Planning Instances", buildOutMissingResolvers);

            
            rebuildReferencedAssemblyArray();
            
            
            var generatedSingletons = AllInstances()
                .OfType<GeneratedInstance>()
                .Where(x => x.Lifetime != ServiceLifetime.Transient && !x.ServiceType.IsOpenGeneric())
                .TopologicalSort(x => x.Dependencies.OfType<GeneratedInstance>())
                .Where(x => x.Lifetime != ServiceLifetime.Transient && !x.ServiceType.IsOpenGeneric()) // to get rid of things that get injected in again
                .Distinct()
                .ToArray();


            if (generatedSingletons.Any())
            {
                var assembly = ToGeneratedAssembly();
                foreach (var instance in generatedSingletons)
                {
                    instance.GenerateResolver(assembly);
                }

                assembly.CompileAll();

                foreach (var instance in generatedSingletons)
                {
                    instance.AttachResolver(_rootScope);
                }
            }

        }

        private void rebuildReferencedAssemblyArray()
        {
            _allAssemblies = AllInstances().SelectMany(x => x.ReferencedAssemblies())
                .Distinct().ToArray();
        }


        private void buildOutMissingResolvers()
        {
            if (_inPlanning) return;

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


        internal GeneratedAssembly ToGeneratedAssembly()
        {
            // TODO -- will need to get at the GenerationRules from somewhere
            var generatedAssembly = new GeneratedAssembly(new GenerationRules("Jasper.Generated"));

            generatedAssembly.Generation.Assemblies.Fill(_allAssemblies);

            return generatedAssembly;
        }

        private bool _inPlanning = false;

        private void planResolutionStrategies()
        {
            while (AllInstances().Where(x => !x.ServiceType.IsOpenGeneric()).Any(x => !x.HasPlanned))
            {
                foreach (var instance in AllInstances().Where(x => !x.HasPlanned).ToArray())
                {
                    instance.CreatePlan(this);
                }
            }
        }

        internal Instance FindInstance(ParameterInfo parameter)
        {
            if (parameter.HasAttribute<NamedAttribute>())
            {
                var att = parameter.GetAttribute<NamedAttribute>();
                if (att.TypeName.IsNotEmpty())
                {
                    var family = _families.Values.ToArray().FirstOrDefault(x => x.FullNameInCode == att.TypeName);
                    return family.InstanceFor(att.Name);
                }

                return FindInstance(parameter.ParameterType, att.Name);
            }

            return FindDefault(parameter.ParameterType);
        }

        private void organizeIntoFamilies(IServiceCollection services)
        {
            services
                .Where(x => !x.ServiceType.HasAttribute<LamarIgnoreAttribute>())

                .GroupBy(x => x.ServiceType)
                .Select(group => buildFamilyForInstanceGroup(services, @group))
                .Each(family => _families.Add(family.ServiceType, family));


        }

        private ServiceFamily buildFamilyForInstanceGroup(IServiceCollection services, IGrouping<Type, ServiceDescriptor> @group)
        {
            if (@group.Key.IsGenericType && !@group.Key.IsOpenGeneric())
            {
                return buildClosedGenericType(@group.Key, services);
            }

            var instances = @group.Select(Instance.For).ToArray();
            return new ServiceFamily(@group.Key, instances);
        }

        private ServiceFamily buildClosedGenericType(Type serviceType, IServiceCollection services)
        {
            var closed = services.Where(x => x.ServiceType == serviceType).Select(Instance.For);

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

            return new ServiceFamily(serviceType, instances);
        }

        public IServiceCollection Services { get; }

        public IEnumerable<Instance> AllInstances()
        {
            return _families.Values.ToArray().SelectMany(x => x.All).ToArray();
        }

        public IReadOnlyDictionary<Type, ServiceFamily> Families => _families;

        public bool HasFamily(Type serviceType)
        {
            return _families.ContainsKey(serviceType);
        }

        public Instance FindInstance(Type serviceType, string name)
        {
            return ResolveFamily(serviceType).InstanceFor(name);
        }

        public ServiceFamily ResolveFamily(Type serviceType)
        {
            if (_families.ContainsKey(serviceType)) return _families[serviceType];

            lock (_familyLock)
            {
                if (_families.ContainsKey(serviceType)) return _families[serviceType];

                return addMissingFamily(serviceType);
            }
        }

        private ServiceFamily addMissingFamily(Type serviceType)
        {
            var family = TryToCreateMissingFamily(serviceType);

            _families.SmartAdd(serviceType, family);

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
            if (_byType.TryFind(serviceType, out Func<Scope, object> resolver))
            {
                return resolver;
            }

            lock (_familyLock)
            {
                if (_byType.TryFind(serviceType, out resolver))
                {
                    return resolver;
                }

                var family = _families.ContainsKey(serviceType)
                    ? _families[serviceType]
                    : addMissingFamily(serviceType);

                var instance = family.Default;
                if (instance == null)
                {
                    resolver = null;
                }
                else if (instance.Lifetime == ServiceLifetime.Singleton)
                {
                    var inner = instance.ToResolver(_rootScope);
                    resolver = s =>
                    {
                        var value = inner(s);
                        Inject(serviceType, value);

                        return value;
                    };
                }
                else
                {
                    resolver = instance.ToResolver(_rootScope);
                }

                _byType = _byType.AddOrUpdate(serviceType, resolver);

                return resolver;
            }
        }

        public Instance FindDefault(Type serviceType)
        {
            if (serviceType.IsSimple()) return null;

            return ResolveFamily(serviceType)?.Default;
        }

        public Instance[] FindAll(Type serviceType)
        {
            return ResolveFamily(serviceType)?.All ?? new Instance[0];
        }

        public bool CouldBuild(Type concreteType)
        {
            var constructorInstance = new ConstructorInstance(concreteType, concreteType, ServiceLifetime.Transient);
            foreach (var policy in InstancePolicies)
            {
                policy.Apply(constructorInstance);
            }
            
            var ctor = constructorInstance.DetermineConstructor(this, out string message);
            
            
            return ctor != null && message.IsEmpty();
        }

        public void Dispose()
        {
            foreach (var instance in AllInstances().OfType<IDisposable>())
            {
                instance.SafeDispose();
            }
        }

        private readonly Stack<Instance> _chain = new Stack<Instance>();
        private AssemblyScanner[] _scanners = new AssemblyScanner[0];
        private Assembly[] _allAssemblies;

        internal void StartingToPlan(Instance instance)
        {
            if (_chain.Contains(instance))
            {
                throw new InvalidOperationException("Bi-directional dependencies detected:" + Environment.NewLine + _chain.Select(x => x.ToString()).Join(Environment.NewLine));
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
            // TODO -- will need to make this more formal somehow
            if (serviceType.IsSimple() || serviceType.IsDateTime() || serviceType == typeof(TimeSpan) || serviceType.IsValueType || serviceType == typeof(DateTimeOffset)) return new ServiceFamily(serviceType);


            return FamilyPolicies.FirstValue(x => x.Build(serviceType, this));
        }

        IServiceFamilyConfiguration IModel.For<T>()
        {
            return ResolveFamily(typeof(T));
        }

        IServiceFamilyConfiguration IModel.For(Type type)
        {
            return ResolveFamily(type);
        }

        IEnumerable<IServiceFamilyConfiguration> IModel.ServiceTypes => _families.Values.ToArray();

        IEnumerable<Instance> IModel.InstancesOf(Type serviceType)
        {
            return FindAll(serviceType);
        }

        IEnumerable<Instance> IModel.InstancesOf<T>()
        {
            return FindAll(typeof(T));
        }

        Type IModel.DefaultTypeFor<T>()
        {
            return FindDefault(typeof(T))?.ImplementationType;
        }

        Type IModel.DefaultTypeFor(Type serviceType)
        {
            return FindDefault(serviceType)?.ImplementationType;
        }

        IEnumerable<Instance> IModel.AllInstances => AllInstances().ToArray();

        T[] IModel.GetAllPossible<T>()
        {
            return AllInstances().ToArray()
                .Where(x => x.ImplementationType.CanBeCastTo(typeof(T)))
                .Select(x => x.Resolve(_rootScope))
                .OfType<T>()
                .ToArray();
        }

        bool IModel.HasRegistrationFor(Type serviceType)
        {
            return FindDefault(serviceType) != null;
        }

        bool IModel.HasRegistrationFor<T>()
        {
            return FindDefault(typeof(T)) != null;
        }

        IEnumerable<AssemblyScanner> IModel.Scanners => _scanners;

        internal void ClearPlanning()
        {
            _chain.Clear();
        }

        public bool CouldResolve(Type type)
        {
            return FindDefault(type) != null;
        }


        public void AppendServices(IServiceCollection services)
        {
            lock (_familyLock)
            {
                applyScanners(services).Wait(TimeSpan.FromSeconds(2));

                services
                    .Where(x => !x.ServiceType.HasAttribute<LamarIgnoreAttribute>())

                    .GroupBy(x => x.ServiceType)
                    .Each(group =>
                    {
                        if (_families.ContainsKey(group.Key))
                        {
                            var family = _families[group.Key];
                            if (family.Append(group) == AppendState.NewDefault)
                            {
                                _byType = _byType.Remove(group.Key);
                            }
                        
                        }
                        else
                        {
                            var family = buildFamilyForInstanceGroup(services, @group);
                            _families.Add(@group.Key, family);
                        }
                    });

                buildOutMissingResolvers();
            
                rebuildReferencedAssemblyArray();
            }

        }

        internal void Inject(ObjectInstance instance)
        {
            if (_families.ContainsKey(instance.ServiceType))
            {
                if (_families[instance.ServiceType].Append(instance) == AppendState.NewDefault)
                {
                    _byType = _byType.Remove(instance.ServiceType);
                }
            }
            else
            {
                var family = new ServiceFamily(instance.ServiceType, instance);
                _families.Add(instance.ServiceType, family);
            }
        }
    }
}
