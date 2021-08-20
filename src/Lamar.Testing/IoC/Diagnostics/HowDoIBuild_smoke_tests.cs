using System;
using Microsoft.Extensions.DependencyInjection;
using StructureMap.Testing.Widget;
using Xunit;
using Xunit.Abstractions;

namespace Lamar.Testing.IoC.Diagnostics
{
    public class HowDoIBuild_smoke_tests
    {
                private readonly ITestOutputHelper _output;

        public HowDoIBuild_smoke_tests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void empty_container()
        {
            var container = Container.Empty();
            var report = container.HowDoIBuild();

            Console.WriteLine(report);
        }

        [Fact]
        public void display_one_service_for_an_interface()
        {
            #region sample_using-HowDoIBuild
            var container = new Container(x =>
            {
                x.For<IEngine>().Use<Hemi>().Named("The Hemi");

                x.For<IEngine>().Add<VEight>().Singleton().Named("V8");
                x.For<IEngine>().Add<FourFiftyFour>();
                x.For<IEngine>().Add<StraightSix>().Scoped();

                x.For<IEngine>().Add(c => new Rotary()).Named("Rotary");
                x.For<IEngine>().Add(c => c.GetService<PluginElectric>());

                x.For<IEngine>().Add(new InlineFour());

                x.For<IEngine>().UseIfNone<VTwelve>();
            });

            Console.WriteLine(container.HowDoIBuild());
            #endregion
            
            _output.WriteLine(container.HowDoIBuild());
        }



        [Fact]
        public void display_one_service_for__a_nested_container()
        {
            var container = new Container(x =>
            {
                x.For<IEngine>().Use<Hemi>().Named("The Hemi");

                x.For<IEngine>().Add<VEight>().Singleton().Named("V8");
                x.For<IEngine>().Add<FourFiftyFour>();
                x.For<IEngine>().Add<StraightSix>().Scoped();

                x.For<IEngine>().Add(c => new Rotary()).Named("Rotary");
                x.For<IEngine>().Add(c => c.GetService<PluginElectric>());

                x.For<IEngine>().Add(new InlineFour());
            });

            Console.WriteLine(container.GetNestedContainer().HowDoIBuild());
        }


        [Fact]
        public void filter_by_assembly()
        {
            var container = new Container(x =>
            {
                x.For<IEngine>().Use<Hemi>().Named("The Hemi");

                x.For<IEngine>().Add<VEight>().Singleton().Named("V8");
                x.For<IEngine>().Add<FourFiftyFour>();
                x.For<IEngine>().Add<StraightSix>().Scoped();

                x.For<IEngine>().Add(c => new Rotary()).Named("Rotary");
                x.For<IEngine>().Add(c => c.GetService<PluginElectric>());

                x.For<IEngine>().Add(new InlineFour());

                x.For<IWidget>().Use<AWidget>();
            });

            Console.WriteLine(container.HowDoIBuild(assembly: typeof(IWidget).Assembly));
        }

        [Fact]
        public void filtering_examples()
        {
            var container = Container.Empty();

            // Filter by the Assembly of the Service Type
            var byAssembly = container.HowDoIBuild(assembly: typeof(IWidget).Assembly);

            // Only report on the specified Service Type
            var byServiceType = container.HowDoIBuild(typeof(IWidget));

            // Filter to Service Type's in the named namespace
            // The 'IsInNamespace' test will include child namespaces
            var byNamespace = container.HowDoIBuild(@namespace: "StructureMap.Testing.Widget");

            // Filter by a case insensitive string.Contains() match
            // against the Service Type name
            var byType = container.HowDoIBuild(typeName: "Widget");
        }

        [Fact]
        public void filter_by_plugin_type()
        {
            var container = new Container(x =>
            {
                x.For<IEngine>().Use<Hemi>().Named("The Hemi");

                x.For<IEngine>().Add<VEight>().Singleton().Named("V8");
                x.For<IEngine>().Add<FourFiftyFour>();
                x.For<IEngine>().Add<StraightSix>().Scoped();

                x.For<IEngine>().Add(c => new Rotary()).Named("Rotary");
                x.For<IEngine>().Add(c => c.GetService<PluginElectric>());

                x.For<IEngine>().Add(new InlineFour());

                x.For<IWidget>().Use<AWidget>();
            });

            Console.WriteLine(container.HowDoIBuild(typeof(IWidget)));
        }

        [Fact]
        public void filter_by_type_name()
        {
            var container = new Container(x =>
            {
                x.For<IEngine>().Use<Hemi>().Named("The Hemi");

                x.For<IEngine>().Add<VEight>().Singleton().Named("V8");
                x.For<IEngine>().Add<FourFiftyFour>();
                x.For<IEngine>().Add<StraightSix>().Scoped();

                x.For<IEngine>().Add(s => new Rotary()).Named("Rotary");
                x.For<IEngine>().Add(c => c.GetService<PluginElectric>());

                x.For<IEngine>().Add(new InlineFour());

                x.For<IWidget>().Use<AWidget>();

                x.For<AWidget>().Use<AWidget>();
            });

            Console.WriteLine(container.HowDoIBuild(typeName: "Widget"));
        }


        [Fact]
        public void filter_by_namespace()
        {
            var container = new Container(x =>
            {
                x.For<IEngine>().Use<Hemi>().Named("The Hemi");

                x.For<IEngine>().Add<VEight>().Singleton().Named("V8");
                x.For<IEngine>().Add<FourFiftyFour>();
                x.For<IEngine>().Add<StraightSix>().Scoped();

                x.For<IEngine>().Add(c => new Rotary()).Named("Rotary");
                x.For<IEngine>().Add(c => c.GetService<PluginElectric>());

                x.For<IEngine>().Add(new InlineFour());

                x.For<IWidget>().Use<AWidget>();

                x.For<AWidget>().Use<AWidget>();
            });

            Console.WriteLine(container.HowDoIBuild(@namespace: "System"));
        }
    }
}