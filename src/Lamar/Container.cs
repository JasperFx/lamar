using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Lamar.Codegen;
using Lamar.IoC;
using Lamar.IoC.Instances;
using Lamar.Scanning;
using Lamar.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar
{
    public class Container : Scope, IContainer, IServiceScopeFactory
    {
        private bool _isDisposing;

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


        public IServiceScope CreateScope()
        {
            return new Scope(ServiceGraph, this);
        }


        public IContainer GetNestedContainer()
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
            ServiceGraph.Dispose();
        }


        public void AssertConfigurationIsValid(AssertMode mode = AssertMode.Full)
        {
            var writer = new StringWriter();
            bool hasErrors = validateConfiguration(writer);

            if (!hasErrors && mode == AssertMode.Full)
            {
                hasErrors = buildAndValidateAll(writer);
            }

            if (hasErrors)
            {
                writer.WriteLine();
                writer.WriteLine();
                writer.WriteLine("The known registrations are:");
                writer.WriteLine(WhatDoIHave());

                throw new ContainerValidationException(writer.ToString());
            }



        }

        private bool buildAndValidateAll(StringWriter writer)
        {
            bool hasErrors = false;

            foreach (var instance in Model.AllInstances.Where(x => x.Lifetime == ServiceLifetime.Singleton))
            {
                try
                {
                    var o = instance.Resolve(this);

                    if (o != null)
                    {
                        foreach (var method in ValidationMethodAttribute.GetValidationMethods(o.GetType()))
                        {
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

            using (var scope = new Scope(ServiceGraph, this))
            {
                foreach (var instance in Model.AllInstances.Where(x => x.Lifetime != ServiceLifetime.Singleton))
                {
                    try
                    {
                        var o = instance.Resolve(this);

                        if (o != null)
                        {
                            foreach (var method in ValidationMethodAttribute.GetValidationMethods(o.GetType()))
                            {
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
            }

            return hasErrors;
        }

        private bool validateConfiguration(StringWriter writer)
        {
            var invalids = Model.AllInstances.Where(x => x.ErrorMessages.Any()).ToArray();

            if (!invalids.Any()) return false;


            foreach (var instance in invalids)
            {
                writer.WriteLine(instance);
                foreach (var message in instance.ErrorMessages)
                {
                    writer.WriteLine(message);
                }

                writer.WriteLine();
                writer.WriteLine();
            }

            return true;
        }

        public void Configure(IServiceCollection services)
        {
            if (services.Any(x => x.ServiceType == typeof(IFamilyPolicy))) throw new InvalidOperationException("Cannot register any IFamilyPolicy objects in Configure()");
            if (services.Any(x => x.ServiceType == typeof(IFamilyPolicy))) throw new InvalidOperationException("Cannot register any IFamilyPolicy objects in Configure()");

            ServiceGraph.AppendServices(services);
        }

        public void Configure(Action<IServiceCollection> configure)
        {
            var services = new ServiceRegistry();
            configure(services);

            Configure(services);
        }
    }

}
