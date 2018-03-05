using System;
using System.Diagnostics;
using System.Linq;
using Baseline;
using Lamar.IoC;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.GenericWidgets;
using StructureMap.Testing.Widget;
using StructureMap.Testing.Widget2;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance
{
    public class container_model_usage
    {
        public container_model_usage()
        {
            container = new Container(x =>
            {
                x.For(typeof(IService<>)).Add(typeof(Service<>));
                x.For(typeof(IService<>)).Add(typeof(Service2<>));

                x.For<IWidget>().Use<AWidget>().Singleton();

                x.AddTransient<Rule, DefaultRule>();
                x.AddTransient<Rule, ARule>();
                x.AddSingleton<Rule>(new ColorRule("red"));
                

                x.For<IEngine>().Use<PushrodEngine>();

                x.For<Startable1>().Use<Startable1>().Singleton();
                x.For<Startable2>().Use<Startable2>();
                x.For<Startable3>().Use<Startable3>();
            });
        }

        public interface IEngine
        {
        }

        public class PushrodEngine : IEngine
        {
        }

        private readonly Container container;

        [Fact]
        public void can_iterate_through_families_including_both_generics_and_normal()
        {
            var serviceTypes = container.Model.ServiceTypes.Select(x => x.ServiceType).ToArray();
            serviceTypes.ShouldContain(typeof(IContainer));
            serviceTypes.ShouldContain(typeof(IServiceProvider));
            serviceTypes.ShouldContain(typeof(Startable1));
            serviceTypes.ShouldContain(typeof(Startable2));
            serviceTypes.ShouldContain(typeof(Startable3));
            serviceTypes.ShouldContain(typeof(Rule));
            serviceTypes.ShouldContain(typeof(IWidget));
            serviceTypes.ShouldContain(typeof(IService<>));
        }

        [Fact]
        public void can_iterate_through_instances_of_pipeline_graph_for_closed_type_from_model()
        {
            container.Model.InstancesOf<Rule>().Count().ShouldBe(3);
        }

        [Fact]
        public void can_iterate_through_instances_of_pipeline_graph_for_closed_type_that_is_not_registered()
        {
            container.Model.InstancesOf<Container>().Count().ShouldBe(0);
        }

        [Fact]
        public void can_iterate_through_instances_of_pipeline_graph_for_generics()
        {
            container.Model.For(typeof(IService<>)).Instances.Count().ShouldBe(2);
        }

        [Fact]
        public void can_iterate_through_instances_of_pipeline_graph_for_generics_from_model()
        {
            container.Model.InstancesOf(typeof(IService<>)).Count().ShouldBe(2);
        }

        [Fact]
        public void default_type_for_from_the_top()
        {
            container.Model.DefaultTypeFor<IWidget>().ShouldBe(typeof(AWidget));
            container.Model.DefaultTypeFor<Rule>().ShouldBe(typeof(ColorRule));
        }

        [Fact]
        public void get_all_instances_from_the_top()
        {
            container.Model.AllInstances.Count().ShouldBeGreaterThan(10); // Func/Func+Arg/Lazy are built in
        }

        [Fact]
        public void get_all_possibles()
        {
            // Startable1 is a SingletonThing

            var startable1 = container.GetInstance<Startable1>();
            startable1.WasStarted.ShouldBeFalse();

            // SAMPLE: calling-startable-start
            var allStartables = container.Model.GetAllPossible<IStartable>();
            allStartables.ToArray()
                .Each(x => x.Start());
            // ENDSAMPLE

            allStartables.Each(x => x.WasStarted.ShouldBeTrue());

            startable1.WasStarted.ShouldBeTrue();
        }

        [Fact]
        public void has_default_implementation_from_the_top()
        {
            container.Model.HasRegistrationFor<IWidget>().ShouldBeTrue();
            container.Model.HasRegistrationFor<Rule>().ShouldBeTrue();
            container.Model.HasRegistrationFor<IServiceProvider>().ShouldBeTrue();
            
            container.Model.HasRegistrationFor<Container>()
                .ShouldBeFalse();
        }

        [Fact]
        public void has_implementation_from_the_top()
        {
            container.Model.HasRegistrationFor<Container>().ShouldBeFalse();
            container.Model.HasRegistrationFor<IWidget>().ShouldBeTrue();
        }

        [Fact]
        public void has_implementations_should_be_false_for_a_type_that_is_not_registered()
        {
            container.Model.For<ISomething>().HasImplementations().ShouldBeFalse();
        }
    }
    
    public interface ISomething
    {
    }

    public class Something : ISomething
    {
        public Something()
        {
            throw new Exception("You can't make me!");
        }
    }

    // SAMPLE: istartable
    public interface IStartable
    {
        bool WasStarted { get; }

        void Start();
    }

    // ENDSAMPLE

    public class Startable : IStartable
    {
        public void Start()
        {
            WasStarted = true;
        }

        public bool WasStarted { get; private set; }
    }

    public class Startable1 : Startable
    {
    }

    public class Startable2 : Startable
    {
    }

    public class Startable3 : Startable
    {
    }
}