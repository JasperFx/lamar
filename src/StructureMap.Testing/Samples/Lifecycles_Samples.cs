﻿using StructureMap.Pipeline;
using Xunit;

namespace StructureMap.Testing.Samples
{
    public class Lifecycles_Samples
    {
        #region sample_SingletonThing-in-action
        [Fact]
        public void singletons()
        {
            var c = new Container(x => { x.For<IService>().Use<Service>().Singleton(); });

            // It's always the same object instance
            c.GetInstance<IService>()
                .ShouldBeTheSameAs(c.GetInstance<IService>())
                .ShouldBeTheSameAs(c.GetInstance<IService>())
                .ShouldBeTheSameAs(c.GetInstance<IService>())
                .ShouldBeTheSameAs(c.GetInstance<IService>())
                .ShouldBeTheSameAs(c.GetInstance<IService>());
        }

        #endregion

        [Fact]
        public void singletons_2()
        {
            var c = new Container(x => { x.For<IService>().Singleton().Use<Service>(); });

            // It's always the same object instance
            var original = c.GetInstance<IService>();
            original
                .ShouldBeTheSameAs(c.GetInstance<IService>())
                .ShouldBeTheSameAs(c.GetInstance<IService>())
                .ShouldBeTheSameAs(c.GetInstance<IService>())
                .ShouldBeTheSameAs(c.GetInstance<IService>())
                .ShouldBeTheSameAs(c.GetInstance<IService>());

            using (var nested = c.GetNestedContainer())
            {
                nested.GetInstance<IService>()
                    .ShouldBeTheSameAs(original);
            }
        }

        #region sample_how-transient-works
        [Fact]
        public void Transient()
        {
            var c = new Container(x => { x.For<IService>().Use<Service>().Transient(); });

            // In a normal container, you get a new object
            // instance of the Service class in subsequent
            // requests
            c.GetInstance<IService>()
                .ShouldNotBeTheSameAs(c.GetInstance<IService>())
                .ShouldNotBeTheSameAs(c.GetInstance<IService>());

            // Within a nested container, 'Transient' now
            // means within the Nested Container.
            // A nested container is effectively one request
            using (var nested = c.GetNestedContainer())
            {
                nested.GetInstance<IService>()
                    .ShouldBeTheSameAs(nested.GetInstance<IService>())
                    .ShouldBeTheSameAs(nested.GetInstance<IService>());
            }
        }

        #endregion

        #region sample_how-always-unique
        [Fact]
        public void Always_Unique()
        {
            var c = new Container(x => { x.For<IService>().Use<Service>().AlwaysUnique(); });

            // In a normal container, you get a new object
            // instance of the Service class in subsequent
            // requests
            c.GetInstance<IService>()
                .ShouldNotBeTheSameAs(c.GetInstance<IService>())
                .ShouldNotBeTheSameAs(c.GetInstance<IService>());

            // Within a nested container, 'Transient' now
            // means within the Nested Container.
            // A nested container is effectively one request
            using (var nested = c.GetNestedContainer())
            {
                nested.GetInstance<IService>()
                    .ShouldNotBeTheSameAs(nested.GetInstance<IService>())
                    .ShouldNotBeTheSameAs(nested.GetInstance<IService>());
            }

            // Even in a single request,
            var holder = c.GetInstance<ServiceUserHolder>();
            holder.Service.ShouldNotBeTheSameAs(holder.User.Service);
        }

        #endregion

        public class ServiceUser
        {
            public IService Service { get; set; }

            public ServiceUser(IService service)
            {
                Service = service;
            }
        }

        public class ServiceUserHolder
        {
            public IService Service { get; set; }
            public ServiceUser User { get; set; }

            public ServiceUserHolder(IService service, ServiceUser user)
            {
                Service = service;
                User = user;
            }
        }

        [Fact]
        public void Transient_within_a_single_request()
        {
            var container = new Container(x => x.For<IService>().Use<Service>());

            var holder = container.GetInstance<ServiceUserHolder>();
            holder.Service.ShouldBeTheSameAs(holder.User.Service);
        }

        #region sample_lifecycle-configuration-at-plugin-type
        public class LifecycleAtPluginTypeRegistry : Registry
        {
            public LifecycleAtPluginTypeRegistry()
            {
                For<IService>().Singleton();

                // This is the default behavior anyway
                For<IGateway>().Transient();

                For<IRule>().AlwaysUnique();

                // ThreadLocal scoping is so rare that SM does
                // not have a convenience method for setting
                // that as the lifecycle
                For<ICache>().LifecycleIs(Lifecycles.ThreadLocal);

                // Use a custom lifecycle
                For<IWeirdThing>().LifecycleIs<MyCustomLifecycle>();
            }
        }

        #endregion

        #region sample_lifecycle-configuration-at-instance
        public class LifecycleByInstanceRegistry : Registry
        {
            public LifecycleByInstanceRegistry()
            {
                For<IService>().Use<Service>().Named("1").Singleton();
                For<IService>().Use<Service>().Named("2").Transient();
                For<IService>().Use<Service>().Named("3").AlwaysUnique();
                For<IService>().Use<Service>().Named("4").LifecycleIs<MyCustomLifecycle>();
            }
        }

        #endregion

        public class MyCustomLifecycle : ILifecycle
        {
            public string Description
            {
                get { return "Some explanatory text for diagnostics"; }
            }

            public void EjectAll(ILifecycleContext context)
            {
                // remove all stored objects from the instance
                // cache and call dispose anything that is IDisposable
            }

            public IObjectCache FindCache(ILifecycleContext context)
            {
                // Using the context, fetch the object cache
                // for the lifecycle
                return new LifecycleObjectCache();
            }
        }
    }

    public interface IService
    {
    }

    public class Service : IService
    {
    }

    public interface IGateway
    {
    }

    public class Gateway : IGateway
    {
    }

    public interface IRule
    {
    }

    public class Rule : IRule
    {
    }

    public interface ICache
    {
    }

    public class Cache
    {
    }

    public interface IWeirdThing
    {
    }

    public class WeirdThing
    {
    }
}