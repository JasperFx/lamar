﻿using StructureMap.Pipeline;
using StructureMap.Testing.Widget;
using System.Diagnostics;
using StructureMap.TypeRules;
using Xunit;

namespace StructureMap.Testing
{
    public class WhatDoIHave_Smoke_Tester
    {
        [Fact]
        public void empty_container()
        {
            #region sample_whatdoihave-simple
            var container = new Container();
            var report = container.WhatDoIHave();

            Debug.WriteLine(report);
            #endregion
        }

        [Fact]
        public void display_one_service_for_an_interface()
        {
            #region sample_what_do_i_have_container
            var container = new Container(x =>
            {
                x.For<IEngine>().Use<Hemi>().Named("The Hemi");

                x.For<IEngine>().Add<VEight>().Singleton().Named("V8");
                x.For<IEngine>().Add<FourFiftyFour>().AlwaysUnique();
                x.For<IEngine>().Add<StraightSix>().LifecycleIs<ThreadLocalStorageLifecycle>();

                x.For<IEngine>().Add(() => new Rotary()).Named("Rotary");
                x.For<IEngine>().Add(c => c.GetInstance<PluginElectric>());

                x.For<IEngine>().Add(new InlineFour());

                x.For<IEngine>().UseIfNone<VTwelve>();
                x.For<IEngine>().MissingNamedInstanceIs.ConstructedBy(c => new NamedEngine(c.RequestedName));
            });
            #endregion

            #region sample_whatdoihave_everything
            Debug.WriteLine(container.WhatDoIHave());
            #endregion
        }

        [Fact]
        public void render_the_fallback_instance_if_it_exists()
        {
            var container = new Container(x =>
            {
                x.For<IEngine>().Use<Hemi>().Named("The Hemi");

                x.For<IEngine>().Add<VEight>().Singleton().Named("V8");
                x.For<IEngine>().Add<FourFiftyFour>().AlwaysUnique();
                x.For<IEngine>().Add<StraightSix>().LifecycleIs<ThreadLocalStorageLifecycle>();

                x.For<IEngine>().Add(() => new Rotary()).Named("Rotary");
                x.For<IEngine>().Add(c => c.GetInstance<PluginElectric>());

                x.For<IEngine>().Add(new InlineFour());

                x.For<IEngine>().UseIfNone<VTwelve>();
                x.For<IEngine>().MissingNamedInstanceIs.ConstructedBy(c => new NamedEngine(c.RequestedName));
            });

            var description = container.WhatDoIHave();

            description.ShouldContain("*Fallback*");
            description.ShouldContain("StructureMap.Testing.VTwelve");
        }

        [Fact]
        public void render_the_missing_named_instance_if_it_exists()
        {
            var container =
                new Container(
                    x =>
                    {
                        x.For<IEngine>().MissingNamedInstanceIs.ConstructedBy(c => new NamedEngine(c.RequestedName));
                    });

            var description = container.WhatDoIHave();
            description.ShouldContain("*Missing Named Instance*");
            description.ShouldContain("Lambda: new NamedEngine(IContext.RequestedName)");
        }

        [Fact]
        public void display_one_service_for__a_nested_container()
        {
            var container = new Container(x =>
            {
                x.For<IEngine>().Use<Hemi>().Named("The Hemi");

                x.For<IEngine>().Add<VEight>().Singleton().Named("V8");
                x.For<IEngine>().Add<FourFiftyFour>().AlwaysUnique();
                x.For<IEngine>().Add<StraightSix>().LifecycleIs<ThreadLocalStorageLifecycle>();

                x.For<IEngine>().Add(() => new Rotary()).Named("Rotary");
                x.For<IEngine>().Add(c => c.GetInstance<PluginElectric>());

                x.For<IEngine>().Add(new InlineFour());
            });

            Debug.WriteLine(container.GetNestedContainer().WhatDoIHave());
        }

        [Fact]
        public void display_one_service_for__a_profile_container()
        {
            var container = new Container(x =>
            {
                x.For<IEngine>().Use<Hemi>().Named("The Hemi");

                x.For<IEngine>().Add<VEight>().Singleton().Named("V8");
                x.For<IEngine>().Add<FourFiftyFour>().AlwaysUnique();
                x.For<IEngine>().Add<StraightSix>().LifecycleIs<ThreadLocalStorageLifecycle>();

                x.For<IEngine>().Add(() => new Rotary()).Named("Rotary");
                x.For<IEngine>().Add(c => c.GetInstance<PluginElectric>());

                x.For<IEngine>().Add(new InlineFour());

                x.Profile("Blue", blue => { blue.For<IEngine>().Use<FourFiftyFour>().Named("Gas Guzzler"); });
            });

            Debug.WriteLine(container.GetProfile("Blue").WhatDoIHave());
        }

        [Fact]
        public void filter_by_assembly()
        {
            var container = new Container(x =>
            {
                x.For<IEngine>().Use<Hemi>().Named("The Hemi");

                x.For<IEngine>().Add<VEight>().Singleton().Named("V8");
                x.For<IEngine>().Add<FourFiftyFour>().AlwaysUnique();
                x.For<IEngine>().Add<StraightSix>().LifecycleIs<ThreadLocalStorageLifecycle>();

                x.For<IEngine>().Add(() => new Rotary()).Named("Rotary");
                x.For<IEngine>().Add(c => c.GetInstance<PluginElectric>());

                x.For<IEngine>().Add(new InlineFour());

                x.For<IWidget>().Use<AWidget>();
            });

            #region sample_whatdoihave-assembly
            Debug.WriteLine(container.WhatDoIHave(assembly: typeof(IWidget).GetAssembly()));
            #endregion
        }

        [Fact]
        public void filtering_examples()
        {
            #region sample_whatdoihave-filtering
            var container = new Container();

            // Filter by the Assembly of the Plugin Type
            var byAssembly = container.WhatDoIHave(assembly: typeof(IWidget).GetAssembly());

            // Only report on the specified Plugin Type
            var byPluginType = container.WhatDoIHave(typeof(IWidget));

            // Filter to Plugin Type's in the named namespace
            // The 'IsInNamespace' test will include child namespaces
            var byNamespace = container.WhatDoIHave(@namespace: "StructureMap.Testing.Widget");

            // Filter by a case insensitive string.Contains() match
            // against the Plugin Type name
            var byType = container.WhatDoIHave(typeName: "Widget");
            #endregion
        }

        [Fact]
        public void filter_by_plugin_type()
        {
            var container = new Container(x =>
            {
                x.For<IEngine>().Use<Hemi>().Named("The Hemi");

                x.For<IEngine>().Add<VEight>().Singleton().Named("V8");
                x.For<IEngine>().Add<FourFiftyFour>().AlwaysUnique();
                x.For<IEngine>().Add<StraightSix>().LifecycleIs<ThreadLocalStorageLifecycle>();

                x.For<IEngine>().Add(() => new Rotary()).Named("Rotary");
                x.For<IEngine>().Add(c => c.GetInstance<PluginElectric>());

                x.For<IEngine>().Add(new InlineFour());

                x.For<IWidget>().Use<AWidget>();
            });

            #region sample_whatdoihave-plugintype
            Debug.WriteLine(container.WhatDoIHave(typeof(IWidget)));
            #endregion
        }

        [Fact]
        public void filter_by_type_name()
        {
            var container = new Container(x =>
            {
                x.For<IEngine>().Use<Hemi>().Named("The Hemi");

                x.For<IEngine>().Add<VEight>().Singleton().Named("V8");
                x.For<IEngine>().Add<FourFiftyFour>().AlwaysUnique();
                x.For<IEngine>().Add<StraightSix>().LifecycleIs<ThreadLocalStorageLifecycle>();

                x.For<IEngine>().Add(() => new Rotary()).Named("Rotary");
                x.For<IEngine>().Add(c => c.GetInstance<PluginElectric>());

                x.For<IEngine>().Add(new InlineFour());

                x.For<IWidget>().Use<AWidget>();

                x.For<AWidget>().Use<AWidget>();
            });

            #region sample_whatdoihave-type
            Debug.WriteLine(container.WhatDoIHave(typeName: "Widget"));
            #endregion
        }

        [Fact]
        public void filter_by_namespace()
        {
            var container = new Container(x =>
            {
                x.For<IEngine>().Use<Hemi>().Named("The Hemi");

                x.For<IEngine>().Add<VEight>().Singleton().Named("V8");
                x.For<IEngine>().Add<FourFiftyFour>().AlwaysUnique();
                x.For<IEngine>().Add<StraightSix>().LifecycleIs<ThreadLocalStorageLifecycle>();

                x.For<IEngine>().Add(() => new Rotary()).Named("Rotary");
                x.For<IEngine>().Add(c => c.GetInstance<PluginElectric>());

                x.For<IEngine>().Add(new InlineFour());

                x.For<IWidget>().Use<AWidget>();

                x.For<AWidget>().Use<AWidget>();
            });

            #region sample_whatdoihave-namespace
            Debug.WriteLine(container.WhatDoIHave(@namespace: "System"));
            #endregion
        }
    }

    public interface IAutomobile
    {
    }

    public interface IEngine
    {
    }

    public class NamedEngine : IEngine
    {
        private readonly string _name;

        public NamedEngine(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }
    }

    public class VEight : IEngine
    {
    }

    public class StraightSix : IEngine
    {
    }

    public class Hemi : IEngine
    {
    }

    public class FourFiftyFour : IEngine
    {
    }

    public class VTwelve : IEngine
    {
    }

    public class Rotary : IEngine
    {
    }

    public class PluginElectric : IEngine
    {
    }

    public class InlineFour : IEngine
    {
        public override string ToString()
        {
            return "I'm an inline 4!";
        }
    }
}