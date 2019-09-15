using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lamar.IoC;
using LamarCodeGeneration;
using LamarCodeGeneration.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar
{
    // SAMPLE: Container-Declaration
    public class Container : Scope, IContainer, INestedContainer, IServiceScopeFactory, IServiceScope,
            ISupportRequiredService
        // ENDSAMPLE
    {
        private bool _isDisposing;

        private Container()
        {
        }

        public Container(IServiceCollection services) : base(services)
        {
        }

        public Container(Action<ServiceRegistry> configuration) : this(ServiceRegistry.For(configuration))
        {
        }

        private Container(ServiceGraph serviceGraph, Container container) : base(serviceGraph, container)
        {
        }

        public Container(IServiceCollection services, PerfTimer timer) : base(services, timer)
        {
        }


        public INestedContainer GetNestedContainer()
        {
            assertNotDisposed();
            return new Container(ServiceGraph, this);
        }

        public override void Dispose()
        {
            // Because a StackOverflowException when trying to cleanly shut down
            // an application is really no fun
            if (_isDisposing) return;

            _isDisposing = true;

            base.Dispose();

            if (ReferenceEquals(Root, this)) ServiceGraph.Dispose();
        }


        public void AssertConfigurationIsValid(AssertMode mode = AssertMode.Full)
        {
            using (var writer = new StringWriter())
            {
                var hasErrors = validateConfiguration(writer);

                if (!hasErrors && mode == AssertMode.Full) hasErrors = buildAndValidateAll(writer);

                if (hasErrors)
                {
                    throw new ContainerValidationException(writer.ToString(), WhatDoIHave(), WhatDidIScan());
                }
            }
        }

        /// <summary>
        ///     Add additional configurations to this container. NOT RECOMMENDED.
        /// </summary>
        /// <param name="services"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Configure(IServiceCollection services)
        {
            if (services.Any(x => x.ServiceType == typeof(IFamilyPolicy)))
                throw new InvalidOperationException("Cannot register any IFamilyPolicy objects in Configure()");
            if (services.Any(x => x.ServiceType == typeof(IFamilyPolicy)))
                throw new InvalidOperationException("Cannot register any IFamilyPolicy objects in Configure()");

            ServiceGraph.AppendServices(services);
        }

        /// <summary>
        ///     Add additional configurations to this container. NOT RECOMMENDED.
        /// </summary>
        /// <param name="configure"></param>
        public void Configure(Action<IServiceCollection> configure)
        {
            if (!ReferenceEquals(this, Root))
                throw new InvalidOperationException("Configure() cannot be used with nested containers");

            var services = new ServiceRegistry();
            configure(services);

            Configure(services);
        }


        IServiceScope IServiceScopeFactory.CreateScope()
        {
            return (IServiceScope) GetNestedContainer();
        }


        object ISupportRequiredService.GetRequiredService(Type serviceType)
        {
            return GetInstance(serviceType);
        }

        public new static Container Empty()
        {
            return For(_ => { });
        }

        public static Container For<T>() where T : ServiceRegistry, new()
        {
            return new Container(new T());
        }

        public static Container For(Action<ServiceRegistry> configuration)
        {
            var registry = new ServiceRegistry();
            configuration(registry);

            return new Container(registry);
        }

        public static Task<Container> BuildAsync(Action<ServiceRegistry> configure, PerfTimer timer = null)
        {
            var services = new ServiceRegistry();
            configure(services);

            return BuildAsync(services, timer);
        }

        public static async Task<Container> BuildAsync(IServiceCollection services, PerfTimer timer = null)
        {
            var timedExternally = true;
            var timerMarker = "Bootstrapping Lamar Container";
            if (timer == null)
            {
                timedExternally = false;
                timer = new PerfTimer();
                timer.Start(timerMarker);
            }
            else
            {
                timer.MarkStart(timerMarker);
            }


            var container = new Container();

            var graph = await ServiceGraph.BuildAsync(services, container);

            container.Root = container;
            container.ServiceGraph = graph;

            container.Bootstrapping = timer;


            graph.Initialize(timer);

            if (timedExternally)
                timer.MarkFinished(timerMarker);
            else
                timer.Stop();

            return container;
        }

        private bool buildAndValidateAll(StringWriter writer)
        {
            var hasErrors = false;

            foreach (var instance in Model.AllInstances.Where(x =>
                x.Lifetime == ServiceLifetime.Singleton && !x.ServiceType.IsOpenGeneric()))
                try
                {
                    var o = instance.Instance.Resolve(this);

                    if (o != null)
                        foreach (var method in ValidationMethodAttribute.GetValidationMethods(o.GetType()))
                            try
                            {
                                method.Invoke(o, new object[0]);
                            }
                            catch (Exception e)
                            {
                                hasErrors = true;

                                writer.WriteLine($"Error in {o.GetType().FullNameInCode()}.{method.Name}()");
                                writer.WriteLine(e.ToString());
                                writer.WriteLine();
                                writer.WriteLine();
                            }
                }
                catch (Exception e)
                {
                    hasErrors = true;

                    writer.WriteLine("Error in " + instance);
                    writer.WriteLine(e.ToString());
                    writer.WriteLine();
                    writer.WriteLine();
                }

            using (var scope = new Scope(ServiceGraph, this))
            {
                foreach (var instance in Model.AllInstances.Where(x =>
                    x.Lifetime != ServiceLifetime.Singleton && !x.ServiceType.IsOpenGeneric()))
                    try
                    {
                        var o = instance.Instance.Resolve(this);

                        if (o != null)
                            foreach (var method in ValidationMethodAttribute.GetValidationMethods(o.GetType()))
                                try
                                {
                                    method.Invoke(o, new object[0]);
                                }
                                catch (Exception e)
                                {
                                    hasErrors = true;

                                    writer.WriteLine($"Error in {o.GetType().FullNameInCode()}.{method.Name}()");
                                    writer.WriteLine(e.ToString());
                                    writer.WriteLine();
                                    writer.WriteLine();
                                }
                    }
                    catch (Exception e)
                    {
                        hasErrors = true;

                        writer.WriteLine("Error in " + instance);
                        writer.WriteLine(e.ToString());
                        writer.WriteLine();
                        writer.WriteLine();
                    }
            }

            return hasErrors;
        }

        private bool validateConfiguration(StringWriter writer)
        {
            var invalids = Model.AllInstances.Where(x => x.Instance.ErrorMessages.Any()).ToArray();

            if (!invalids.Any()) return false;


            foreach (var instance in invalids)
            {
                writer.WriteLine(instance);
                foreach (var message in instance.Instance.ErrorMessages) writer.WriteLine(message);

                writer.WriteLine();
                writer.WriteLine();
            }

            return true;
        }
    }

    /// <summary>
    ///     Use internally by Lamar
    /// </summary>
    public interface IStub
    {
    }
}