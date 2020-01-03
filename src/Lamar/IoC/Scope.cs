using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BaselineTypeDiscovery;
using Lamar.IoC.Diagnostics;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;
using Lamar.Scanning;
using LamarCodeGeneration;
using LamarCodeGeneration.Model;
using LamarCodeGeneration.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.IoC
{
    // SAMPLE: Scope-Declarations
    public class Scope : IServiceContext
    // ENDSAMPLE
    {
        protected bool _hasDisposed;

        public static Scope Empty()
        {
            return new Scope(new ServiceRegistry());
        }

        public PerfTimer Bootstrapping { get; protected set; }

        public Scope(IServiceCollection services, PerfTimer timer = null)
        {
            if (timer == null)
            {
                Bootstrapping = new PerfTimer();

                Bootstrapping.Start("Bootstrapping Container");
            }
            else
            {
                Bootstrapping = timer;
                Bootstrapping.MarkStart("Lamar Scope Creation");
            }



            Root = this;

            Bootstrapping.MarkStart("Build ServiceGraph");
            ServiceGraph = new ServiceGraph(services, this);
            Bootstrapping.MarkFinished("Build ServiceGraph");

            ServiceGraph.Initialize(Bootstrapping);

            if (timer == null)
            {
                Bootstrapping.Stop();
            }
            else
            {
                Bootstrapping.MarkFinished("Lamar Scope Creation");
            }


        }

        protected Scope(){}
        
        public Scope Root { get; protected set; }

        

        public Scope(ServiceGraph serviceGraph, Scope root)
        {
            ServiceGraph = serviceGraph;
            Root = root ?? throw new ArgumentNullException(nameof(root));
        }

        /// <summary>
        /// Asserts that this container is not disposed yet.
        /// </summary>
        /// <exception cref="ObjectDisposedException">If the container is disposed.</exception>
        protected void assertNotDisposed()
        {
            if (!_hasDisposed) return;

            throw new ObjectDisposedException("This Container has been disposed");
        }


        public DisposalLock DisposalLock { get; set; } = DisposalLock.Unlocked;


        public IModel Model => new QueryModel(this);

        internal ServiceGraph ServiceGraph { get; set;}


        public ConcurrentBag<IDisposable> Disposables { get; } = new ConcurrentBag<IDisposable>();

        internal readonly Dictionary<int, object> Services = new Dictionary<int, object>();

        

        public virtual void Dispose()
        {
            if (DisposalLock == DisposalLock.Ignore) return;

            if (DisposalLock == DisposalLock.ThrowOnDispose) throw new InvalidOperationException("This Container has DisposalLock = DisposalLock.ThrowOnDispose and cannot be disposed until the lock is cleared");

            if (_hasDisposed) return;
            _hasDisposed = true;

            foreach (var disposable in Disposables.Distinct())
            {
                disposable.SafeDispose();
            }
        }

        public IServiceProvider ServiceProvider => this;

        public object GetService(Type serviceType)
        {
            return TryGetInstance(serviceType);
        }

        public T GetInstance<T>()
        {
            return (T) GetInstance(typeof(T));
        }

        public T GetInstance<T>(string name)
        {
            return (T) GetInstance(typeof(T), name);
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
            return (T) QuickBuild(typeof(T));

        }

        public object QuickBuild(Type objectType)
        {
            assertNotDisposed();

            if (!objectType.IsConcrete()) throw new InvalidOperationException("Type must be concrete");

            var constructorInstance = new ConstructorInstance(objectType, objectType, ServiceLifetime.Transient);
            var ctor = constructorInstance.DetermineConstructor(ServiceGraph, out var message);
            var setters = constructorInstance.FindSetters(ServiceGraph);
            
            
            if (ctor == null) throw new InvalidOperationException(message);

            var dependencies = ctor.GetParameters().Select(x =>
            {
                var instance = ServiceGraph.FindInstance(x);

                if (instance == null) throw new InvalidOperationException($"Cannot QuickBuild type {objectType.GetFullName()} because Lamar cannot determine how to build required dependency {x.ParameterType.FullNameInCode()}");

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
            foreach (var setter in setters)
            {
                setter.ApplyQuickBuildProperties(service, this);
            }
            
            return service;
        }

        public IReadOnlyList<T> QuickBuildAll<T>()
        {
            assertNotDisposed();
            return ServiceGraph.FindAll(typeof(T)).Select(x => x.QuickResolve(this)).OfType<T>().ToList();
        }

        public void BuildUp(object target)
        {
            var objectType = target.GetType();
            var constructorInstance = new ConstructorInstance(objectType, objectType, ServiceLifetime.Transient);
            var setters = constructorInstance.FindSetters(ServiceGraph);

            foreach (var setter in setters)
            {
                setter.ApplyQuickBuildProperties(target, this);
            }
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
        /// Returns a textual report of all the assembly scanners used to build up this Container
        /// </summary>
        /// <returns></returns>
        public string WhatDidIScan()
        {
            assertNotDisposed();

            var scanners = Model.Scanners;

            if (!scanners.Any()) return "No type scanning in this Container";

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
        
        

        public string GenerateCodeWithInlineServices(GeneratedAssembly assembly)
        {
            return assembly.GenerateCode(new ServiceVariableSource(ServiceGraph));
        }

        // don't build this if you don't need it
        private Dictionary<Type, object> _injected;

        public virtual void Inject( Type serviceType, object @object, bool replace )
        {
            if ( !serviceType.IsAssignableFrom( @object.GetType() ) )
                throw new InvalidOperationException( $"{serviceType} is not assignable from {@object.GetType()}" );

            if ( _injected == null )
            {
                _injected = new Dictionary<Type, object>();
            }

            if ( replace ) 
            {
                _injected[serviceType] = @object;
            }

            else
            {
                _injected.Add( serviceType, @object );
            }
        }

        public void Inject<T>( T @object ) => Inject( typeof(T), @object, false );
        public void Inject<T>( T @object, bool replace = false ) => Inject( typeof(T), @object, replace );
        
        public T GetInjected<T>()
        {
            return (T) (_injected?.ContainsKey(typeof(T)) ?? false ? _injected[typeof(T)] : null);
        }

        /// <summary>
        /// Some bookkeeping here. Tracks this to the scope's disposable tracking *if* it is disposable
        /// </summary>
        /// <param name="object"></param>
        public void TryAddDisposable(object @object)
        {
            if (@object is IDisposable disposable)
            {
                Disposables.Add(disposable);
            }
        }
        
        public object AddDisposable(object @object)
        {
            Disposables.Add((IDisposable) @object);
            return @object;
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
    }
}
