using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ImTools;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Model;
using JasperFx.Core;
using JasperFx.Core.Reflection;
using JasperFx.Core.TypeScanning;
using Lamar.Diagnostics;
using Lamar.IoC.Diagnostics;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.IoC;

#region sample_Scope-Declarations

public class Scope : IServiceContext, IServiceProviderIsKeyedService

    #endregion

{
    protected bool _hasDisposed;

    // don't build this if you don't need it
    private Dictionary<Type, object> _injected;

    internal InstanceMap Services;

    public Scope(IServiceCollection services) : this(services, InstanceMapBehavior.Default) {}

    public Scope(IServiceCollection services, InstanceMapBehavior instanceMapBehavior)
    {
        Services = new InstanceMap(instanceMapBehavior);

        Root = this;

        ServiceGraph = new ServiceGraph(services, this);

        ServiceGraph.Initialize();
    }

    protected Scope() : this(InstanceMapBehavior.Default) { }

    protected Scope(InstanceMapBehavior instanceMapBehavior = InstanceMapBehavior.Default)
    {
        Services = new InstanceMap(instanceMapBehavior);
    }

    public Scope(ServiceGraph serviceGraph, Scope root) : this(serviceGraph, root, InstanceMapBehavior.Default) { }
    
    public Scope(ServiceGraph serviceGraph, Scope root, InstanceMapBehavior instanceMapBehavior = InstanceMapBehavior.Default)
    {
        Services = new InstanceMap(instanceMapBehavior);
        ServiceGraph = serviceGraph;
        Root = root ?? throw new ArgumentNullException(nameof(root));
    }

    public Scope Root { get; protected set; }


    public DisposalLock DisposalLock { get; set; } = DisposalLock.Unlocked;

    internal ServiceGraph ServiceGraph { get; set; }


    public ConcurrentBag<IDisposable> Disposables { get; } = new();

    internal IEnumerable<IDisposable> AllDisposables => Disposables;

    public IServiceProvider ServiceProvider => this;


    public IModel Model => new QueryModel(this);

    public virtual void Dispose()
    {
        if (DisposalLock == DisposalLock.ThrowOnDispose)
        {
            throw new InvalidOperationException(
                "This Container has DisposalLock = DisposalLock.ThrowOnDispose and cannot be disposed until the lock is cleared");
        }

        if (_hasDisposed)
        {
            return;
        }

        _hasDisposed = true;

        var distinctDisposables = Disposables.Distinct().ToArray();
        // clear disposables bag to prevent memory leak. current implementation of ConcurrentBag is using thread local storage and in some cases
        // e.g. an object from Disposables collection is referencing this Scope instance the whole graph can stay in memory after it was disposed
        while (Disposables.TryTake(out _))
        {
        }

        if (DisposalLock == DisposalLock.Ignore)
        {
            return;
        }

        foreach (var disposable in distinctDisposables) disposable.SafeDispose();
    }

    public virtual async ValueTask DisposeAsync()
    {
        if (DisposalLock == DisposalLock.ThrowOnDispose)
        {
            throw new InvalidOperationException(
                "This Container has DisposalLock = DisposalLock.ThrowOnDispose and cannot be disposed until the lock is cleared");
        }

        if (_hasDisposed)
        {
            return;
        }

        _hasDisposed = true;

        var distinctDisposables = Disposables.Distinct().ToArray();
        // clear disposables bag to prevent memory leak. current implementation of ConcurrentBag is using thread local storage and in some cases
        // e.g. an object from Disposables collection is referencing this Scope instance the whole graph can stay in memory after it was disposed
        while (Disposables.TryTake(out _))
        {
        }

        if (DisposalLock == DisposalLock.Ignore)
        {
            return;
        }

        foreach (var disposable in distinctDisposables)
        {
            if (disposable is IAsyncDisposable asyncDisposable)
            {
                try
                {
                    await asyncDisposable.DisposeAsync();
                }
                catch (Exception)
                {
                    // Yup, don't let that go out
                }
            }
            else
            {
                disposable.SafeDispose();
            }
        }
    }

    public object GetService(Type serviceType)
    {
        return TryGetInstance(serviceType);
    }

    public T GetInstance<T>()
    {
        return (T)GetInstance(typeof(T));
    }

    public T GetInstance<T>(string name)
    {
        return (T)GetInstance(typeof(T), name);
    }

    public object GetInstance(Type serviceType)
    {
        assertNotDisposed();
        var resolver = ServiceGraph.FindResolver(serviceType);

        if (resolver == null)
        {
            if (ServiceGraph.Families.TryGetValue(serviceType, out var family))
            {
                if (family.CannotBeResolvedMessage.IsNotEmpty())
                {
                    throw new LamarMissingRegistrationException(family);
                }
            }

            throw new LamarMissingRegistrationException(serviceType);
        }

        return resolver(this);
    }

    public object GetInstance(Type serviceType, string name)
    {
        assertNotDisposed();

        var instance = ServiceGraph.FindInstance(serviceType, name);
        if (instance == null)
        {
            throw new LamarMissingRegistrationException(serviceType, name);
        }

        return instance.Resolve(this);
    }

    public T TryGetInstance<T>()
    {
        return (T)(TryGetInstance(typeof(T)) ?? default(T));
    }

    public T TryGetInstance<T>(string name)
    {
        return (T)(TryGetInstance(typeof(T), name) ?? default(T));
    }

    public object TryGetInstance(Type serviceType)
    {
        assertNotDisposed();
        return ServiceGraph.FindResolver(serviceType)?.Invoke(this);
    }

    public object TryGetInstance(Type serviceType, string name)
    {
        assertNotDisposed();
        var instance = ServiceGraph.FindInstance(serviceType, name);
        return instance?.Resolve(this);
    }

    public T QuickBuild<T>()
    {
        return (T)QuickBuild(typeof(T));
    }

    public object QuickBuild(Type objectType)
    {
        assertNotDisposed();

        if (!objectType.IsConcrete())
        {
            throw new InvalidOperationException("Type must be concrete");
        }

        var constructorInstance = new ConstructorInstance(objectType, objectType, ServiceLifetime.Transient);
        var ctor = constructorInstance.DetermineConstructor(ServiceGraph, out var message);
        var setters = constructorInstance.FindSetters(ServiceGraph);

        if (ctor == null)
        {
            throw new InvalidOperationException(message);
        }

        var dependencies = ctor.GetParameters().Select(x =>
        {
            var instance = ServiceGraph.FindInstance(x);

            if (instance == null)
            {
                throw new InvalidOperationException(
                    $"Cannot QuickBuild type {objectType.FullNameInCode()} because Lamar cannot determine how to build required dependency {x.ParameterType.FullNameInCode()}");
            }

            try
            {
                return instance.QuickResolve(this);
            }
            catch (Exception)
            {
                // #sadtrombone, do it the heavy way instead
                return instance.Resolve(this);
            }
        }).ToArray();

        var service = ctor.Invoke(dependencies);
        foreach (var setter in setters) setter.ApplyQuickBuildProperties(service, this);

        return service;
    }

    public IReadOnlyList<T> QuickBuildAll<T>()
    {
        assertNotDisposed();
        return ServiceGraph.FindAll(typeof(T)).Select(x => x.QuickResolve(this)).OfType<T>().ToList();
    }

    public IReadOnlyList<T> GetAllInstances<T>()
    {
        assertNotDisposed();
        return ServiceGraph.FindAll(typeof(T)).Select(x => x.Resolve(this)).OfType<T>().ToList();
    }

    public IEnumerable GetAllInstances(Type serviceType)
    {
        assertNotDisposed();
        return ServiceGraph.FindAll(serviceType).Select(x => x.Resolve(this)).ToArray();
    }


    public string WhatDoIHave(Type serviceType = null, Assembly assembly = null, string @namespace = null,
        string typeName = null)
    {
        assertNotDisposed();

        var writer = new WhatDoIHaveWriter(Model);
        return writer.GetText(new ModelQuery
        {
            Assembly = assembly,
            Namespace = @namespace,
            ServiceType = serviceType,
            TypeName = typeName
        });
    }

    public string HowDoIBuild(Type serviceType = null, Assembly assembly = null, string @namespace = null,
        string typeName = null)
    {
        assertNotDisposed();

        var writer = new WhatDoIHaveWriter(Model);
        return writer.GetText(new ModelQuery
        {
            Assembly = assembly,
            Namespace = @namespace,
            ServiceType = serviceType,
            TypeName = typeName
        }, display: WhatDoIHaveDisplay.BuildPlan);
    }

    /// <summary>
    ///     Returns a textual report of all the assembly scanners used to build up this Container
    /// </summary>
    /// <returns></returns>
    public string WhatDidIScan()
    {
        assertNotDisposed();

        var scanners = Model.Scanners;

        if (!scanners.Any())
        {
            return "No type scanning in this Container";
        }

        using (var writer = new StringWriter())
        {
            writer.WriteLine("All Scanners");
            writer.WriteLine("================================================================");

            scanners.Each(scanner =>
            {
                scanner.Describe(writer);

                writer.WriteLine();
                writer.WriteLine();
            });

            var failed = TypeRepository.FailedAssemblies();
            if (failed.Any())
            {
                writer.WriteLine();
                writer.WriteLine("Assemblies that failed in the call to Assembly.GetExportedTypes()");
                failed.Each(assem => { writer.WriteLine("* " + assem.Record.Name); });
            }
            else
            {
                writer.WriteLine("No problems were encountered in exporting types from Assemblies");
            }

            return writer.ToString();
        }
    }

    public IServiceVariableSource CreateServiceVariableSource()
    {
        return new ServiceVariableSource(ServiceGraph);
    }

    public bool IsService(Type serviceType)
    {
        return ServiceGraph.CanBeServiceByNetCoreRules(serviceType);
    }

    public bool IsKeyedService(Type serviceType, object serviceKey)
    {
        if (serviceKey is string serviceKeyString)
        {
            return ServiceGraph.CanBeServiceByNetCoreRules(serviceType, serviceKeyString);
        }

        throw new ArgumentException("Lamar only supports strings for service keys on typed services",
            nameof(serviceKey));
    }

    public static Scope Empty()
    {
        return new Scope(new ServiceRegistry());
    }

    /// <summary>
    ///     Asserts that this container is not disposed yet.
    /// </summary>
    /// <exception cref="ObjectDisposedException">If the container is disposed.</exception>
    protected void assertNotDisposed()
    {
        if (!_hasDisposed)
        {
            return;
        }

        throw new ObjectDisposedException("This Container has been disposed");
    }

    public void BuildUp(object target)
    {
        var objectType = target.GetType();
        var constructorInstance = new ConstructorInstance(objectType, objectType, ServiceLifetime.Transient);
        var setters = constructorInstance.FindSetters(ServiceGraph);

        foreach (var setter in setters) setter.ApplyQuickBuildProperties(target, this);
    }

    public string GenerateCodeWithInlineServices(GeneratedAssembly assembly)
    {
        return assembly.GenerateCode(new ServiceVariableSource(ServiceGraph));
    }

    public virtual void Inject(Type serviceType, object @object, bool replace)
    {
        if (!serviceType.IsAssignableFrom(@object.GetType()))
        {
            throw new InvalidOperationException($"{serviceType} is not assignable from {@object.GetType()}");
        }

        if (_injected == null)
        {
            _injected = new Dictionary<Type, object>();
        }

        if (replace)
        {
            _injected[serviceType] = @object;
        }

        else
        {
            _injected.Add(serviceType, @object);
        }
    }

    public void Inject<T>(T @object)
    {
        Inject(typeof(T), @object, false);
    }

    public void Inject<T>(T @object, bool replace = false)
    {
        Inject(typeof(T), @object, replace);
    }

    public T GetInjected<T>()
    {
        return (T)(_injected?.ContainsKey(typeof(T)) ?? false ? _injected[typeof(T)] : null);
    }

    /// <summary>
    ///     Some bookkeeping here. Tracks this to the scope's disposable tracking *if* it is disposable
    /// </summary>
    /// <param name="object"></param>
    public void TryAddDisposable(object @object)
    {
        switch (@object)
        {
            case IDisposable disposable:
                Disposables.Add(disposable);
                break;
            case IAsyncDisposable a:
                Disposables.Add(new AsyncDisposableWrapper(a));
                break;
        }
    }

    public Func<string, T> FactoryByNameFor<T>()
    {
        return GetInstance<T>;
    }

    public Func<T> FactoryFor<T>()
    {
        return GetInstance<T>;
    }

    public Lazy<T> LazyFor<T>()
    {
        return new Lazy<T>(GetInstance<T>);
    }
    
    public object GetKeyedService(Type serviceType, object serviceKey)
    {
        return TryGetInstance(serviceType, serviceKey.ToString());
    }

    public object GetRequiredKeyedService(Type serviceType, object serviceKey)
    {
        return GetInstance(serviceType, serviceKey.ToString());
    }
}