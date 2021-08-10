using Shouldly;
using StructureMap.Testing.Widget3;
using System;
using Xunit;

namespace StructureMap.Testing.Configuration.DSL
{
    public class InterceptAllInstancesOfPluginTypeTester : Registry
    {
        public InterceptAllInstancesOfPluginTypeTester()
        {
            _lastService = null;
            _manager = null;

            _defaultRegistry = (registry =>
            {
                //registry.For<IService>()
                //    .AddInstances(
                //    Instance<ColorService>().Named("Red").Ctor<string>("color").
                //        EqualTo(
                //        "Red"),
                //    Object<IService>(new ColorService("Yellow")).Named("Yellow"),
                //    ConstructedBy<IService>(
                //        delegate { return new ColorService("Purple"); })
                //        .Named("Purple"),
                //    Instance<ColorService>().Named("Decorated").Ctor<string>("color")
                //        .
                //        EqualTo("Orange")
                //    );

                #region sample_Using-AddInstances

                // registry is a StructureMap Registry object
                registry.For<IService>().AddInstances(x =>
                {
                    // Equivalent to For<IService>().Add<ColorService>().....
                    x.Type<ColorService>().Named("Red").Ctor<string>("color").Is("Red");

                    // Equivalent to For<IService>().Add(new ColorService("Yellow"))......
                    x.Object(new ColorService("Yellow")).Named("Yellow");

                    // Equivalent to For<IService>().Use(() => new ColorService("Purple"))....
                    x.ConstructedBy(() => new ColorService("Purple")).Named("Purple");

                    x.Type<ColorService>().Named("Decorated").Ctor<string>("color").Is("Orange");
                });

                #endregion
            });
        }

        private IService _lastService;
        private IContainer _manager;
        private Action<Registry> _defaultRegistry;

        private IService getService(string name, Action<Registry> action)
        {
            if (_manager == null)
            {
                _manager = new Container(registry =>
                {
                    _defaultRegistry(registry);
                    action(registry);
                });
            }

            return _manager.GetInstance<IService>(name);
        }

        [Fact]
        public void DecorateForAll()
        {
            var green = getService("Green", r =>
            {
                r.For<IService>().DecorateAllWith(s => new DecoratorService(s))
                    .AddInstances(x => { x.ConstructedBy(() => new ColorService("Green")).Named("Green"); });
            });

            green.ShouldBeOfType<DecoratorService>()
                .Inner.ShouldBeOfType<ColorService>().Color.ShouldBe("Green");
        }

        [Fact]
        public void OnStartupForAll()
        {
            Action<Registry> action = registry =>
            {
                registry.For<IService>().OnCreationForAll("setting the last service", s => _lastService = s)
                    .AddInstances(x => { x.ConstructedBy(() => new ColorService("Green")).Named("Green"); });
            };

            var red = getService("Red", action);
            red.ShouldBeTheSameAs(_lastService);

            var purple = getService("Purple", action);
            purple.ShouldBeTheSameAs(_lastService);

            var green = getService("Green", action);
            green.ShouldBeTheSameAs(_lastService);

            var yellow = getService("Yellow", action);
            _lastService.ShouldBe(yellow);
        }
    }

    public class InterceptAllInstancesOfPluginTypeTester_with_SmartInstance : Registry
    {
        public InterceptAllInstancesOfPluginTypeTester_with_SmartInstance()
        {
            _lastService = null;
            _manager = null;

            _defaultRegistry = (registry =>
                registry.For<IService>().AddInstances(x =>
                {
                    x.Type<ColorService>().Named("Red")
                        .Ctor<string>("color").Is("Red");

                    x.Object(new ColorService("Yellow")).Named("Yellow");

                    x.ConstructedBy(() => new ColorService("Purple")).Named("Purple");

                    x.Type<ColorService>().Named("Decorated").Ctor<string>("color").Is(
                        "Orange");
                }));
        }

        private IService _lastService;
        private IContainer _manager;
        private Action<Registry> _defaultRegistry;

        private IService getService(Action<Registry> action, string name)
        {
            if (_manager == null)
            {
                _manager = new Container(registry =>
                {
                    _defaultRegistry(registry);
                    action(registry);
                });
            }

            return _manager.GetInstance<IService>(name);
        }

        [Fact]
        public void DecorateForAll()
        {
            Action<Registry> action = r =>
            {
                r.For<IService>().DecorateAllWith(s => new DecoratorService(s))
                    .AddInstances(x => { x.ConstructedBy(() => new ColorService("Green")).Named("Green"); });
            };

            var green = getService(action, "Green");

            var decoratorService = (DecoratorService)green;
            var innerService = (ColorService)decoratorService.Inner;
            innerService.Color.ShouldBe("Green");
        }

        [Fact]
        public void OnStartupForAll()
        {
            Action<Registry> action = r =>
            {
                r.For<IService>().OnCreationForAll("setting the last service", s => _lastService = s)
                    .AddInstances(x => x.ConstructedBy(() => new ColorService("Green")).Named("Green"));
            };

            var red = getService(action, "Red");
            red.ShouldBeTheSameAs(_lastService);

            var purple = getService(action, "Purple");
            purple.ShouldBeTheSameAs(_lastService);

            var green = getService(action, "Green");
            green.ShouldBeTheSameAs(_lastService);

            var yellow = getService(action, "Yellow");
            _lastService.ShouldBe(yellow);
        }
    }
}