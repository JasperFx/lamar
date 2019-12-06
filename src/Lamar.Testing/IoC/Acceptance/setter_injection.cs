using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using StructureMap.Testing.Widget3;
using Xunit;
using Xunit.Abstractions;

namespace Lamar.Testing.IoC.Acceptance
{
    public class setter_injection
    {
        private readonly ITestOutputHelper _output;

        public setter_injection(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Singleton)]
        public void build_with_setter_attribute(ServiceLifetime parentLifetime, ServiceLifetime childLifetime)
        {
            var container = Container.For(_ =>
            {
                _.For<IWidget>().Use<AWidget>().Lifetime = childLifetime;
                _.ForConcreteType<GuyWithWidgetSetterAttribute>().Configure.Lifetime = parentLifetime;
            });
            
            _output.WriteLine(container.Model.For<GuyWithWidgetSetterAttribute>().Default.DescribeBuildPlan());

            var guy = container.GetInstance<GuyWithWidgetSetterAttribute>();
            guy.Widget.ShouldBeOfType<AWidget>();
        }
        
        [Theory]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Singleton)]
        public void quick_build_with_setter_attribute(ServiceLifetime parentLifetime, ServiceLifetime childLifetime)
        {
            var container = Container.For(_ =>
            {
                _.For<IWidget>().Use<AWidget>().Lifetime = childLifetime;
                _.ForConcreteType<GuyWithWidgetSetterAttribute>().Configure.Lifetime = parentLifetime;
            });

            var guy = container.QuickBuild<GuyWithWidgetSetterAttribute>();
            guy.Widget.ShouldBeOfType<AWidget>();
        }

        [Fact]
        public void buildup_with_setter_attribute()
        {
            var container = Container.For(_ =>
            {
                _.For<IWidget>().Use<AWidget>().Lifetime = ServiceLifetime.Transient;
            });
            
            var guy = new GuyWithWidgetSetterAttribute();
            container.BuildUp(guy);

            guy.Widget.ShouldBeOfType<AWidget>();
        }
        
        [Theory]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Singleton)]
        public void build_with_setter_of_all_types(ServiceLifetime parentLifetime, ServiceLifetime childLifetime)
        {
            var container = Container.For(_ =>
            {
                _.ForConcreteType<GuyWithWidgetSetter>().Configure.Lifetime = parentLifetime;
                _.Policies.FillAllPropertiesOfType<IWidget>().Use<AWidget>().Lifetime = childLifetime;
            });

            var guy = container.GetInstance<GuyWithWidgetSetter>();
            guy.Widget.ShouldBeOfType<AWidget>();
        }
        
        [Theory]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Singleton)]
        public void build_with_explicit_inline_dependency_for_setter(ServiceLifetime parentLifetime, ServiceLifetime childLifetime)
        {
            var container = Container.For(_ =>
            {
                var instance = _.ForConcreteType<GuyWithWidgetSetter>().Configure;
                instance.Ctor<IWidget>().Is<AWidget>();
                instance.Lifetime = parentLifetime;
            });

            var guy = container.GetInstance<GuyWithWidgetSetter>();
            guy.Widget.ShouldBeOfType<AWidget>();
        }
        
        [Theory]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Singleton)]
        public void build_with_setter_of_policy_of_type(ServiceLifetime parentLifetime, ServiceLifetime childLifetime)
        {
            var container = Container.For(_ =>
            {
                _.For<IWidget>().Use<AWidget>().Lifetime = childLifetime;
                _.ForConcreteType<GuyWithWidgetSetter>().Configure.Lifetime = parentLifetime;
                _.Policies.SetAllProperties(x => x.OfType<IWidget>());
            });

            var guy = container.GetInstance<GuyWithWidgetSetter>();
            guy.Widget.ShouldBeOfType<AWidget>();
        }
        
        [Theory]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Singleton)]
        public void build_with_setter_of_policy_type_matches(ServiceLifetime parentLifetime, ServiceLifetime childLifetime)
        {
            var container = Container.For(_ =>
            {
                _.For<IWidget>().Use<AWidget>().Lifetime = childLifetime;
                _.ForConcreteType<GuyWithWidgetSetter>().Configure.Lifetime = parentLifetime;
                _.Policies.SetAllProperties(x => x.TypeMatches(t => t == typeof(IWidget)));
            });

            var guy = container.GetInstance<GuyWithWidgetSetter>();
            guy.Widget.ShouldBeOfType<AWidget>();
        }

        [Theory]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Singleton)]
        public void build_with_setter_of_policy_matching_prop(ServiceLifetime parentLifetime, ServiceLifetime childLifetime)
        {
            var container = Container.For(_ =>
            {
                _.For<IWidget>().Use<AWidget>().Lifetime = childLifetime;
                _.ForConcreteType<GuyWithWidgetSetter>().Configure.Lifetime = parentLifetime;
                _.Policies.SetAllProperties(x => x.Matching(p => p.Name == "Widget"));
            });

            var guy = container.GetInstance<GuyWithWidgetSetter>();
            guy.Widget.ShouldBeOfType<AWidget>();
        }
        
        [Theory]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Singleton)]
        public void build_with_setter_of_policy_with_any_type_from_namespace(ServiceLifetime parentLifetime, ServiceLifetime childLifetime)
        {
            var container = Container.For(_ =>
            {
                _.For<IWidget>().Use<AWidget>().Lifetime = childLifetime;
                _.ForConcreteType<GuyWithWidgetSetter>().Configure.Lifetime = parentLifetime;
                _.Policies.SetAllProperties(x => x.WithAnyTypeFromNamespace(typeof(IWidget).Namespace));
            });

            var guy = container.GetInstance<GuyWithWidgetSetter>();
            guy.Widget.ShouldBeOfType<AWidget>();
        }

        [Theory]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Singleton)]
        public void build_with_setter_of_policy_with_any_type_from_namespace_containing_type(ServiceLifetime parentLifetime, ServiceLifetime childLifetime)
        {
            var container = Container.For(_ =>
            {
                _.For<IWidget>().Use<AWidget>().Lifetime = childLifetime;
                _.ForConcreteType<GuyWithWidgetSetter>().Configure.Lifetime = parentLifetime;
                _.Policies.SetAllProperties(x => x.WithAnyTypeFromNamespaceContainingType<IWidget>());
            });

            var guy = container.GetInstance<GuyWithWidgetSetter>();
            guy.Widget.ShouldBeOfType<AWidget>();
        }
        
        [Theory]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Singleton)]
        public void build_with_setter_of_policy_with_name_matches(ServiceLifetime parentLifetime, ServiceLifetime childLifetime)
        {
            var container = Container.For(_ =>
            {
                _.For<IWidget>().Use<AWidget>().Lifetime = childLifetime;
                _.ForConcreteType<GuyWithWidgetSetter>().Configure.Lifetime = parentLifetime;
                _.Policies.SetAllProperties(x => x.NameMatches(n => n == "Widget"));
            });

            var guy = container.GetInstance<GuyWithWidgetSetter>();
            guy.Widget.ShouldBeOfType<AWidget>();
        }

        public class GuyWithWidgetSetterAttribute
        {
            [SetterProperty]
            public IWidget Widget { get; set; }
        }
        
        public class GuyWithWidgetSetter
        {
            public IWidget Widget { get; set; }
        }
    }
    
    public class ConventionBasedSetterInjectionTester
    {
        // SAMPLE: using-setter-policy
        public class ClassWithNamedProperties
        {
            public int Age { get; set; }
            public string LastName { get; set; }
            public string FirstName { get; set; }
            public IGateway Gateway { get; set; }
            public IService Service { get; set; }
        }

        [Fact]
        public void specify_setter_policy_and_construct_an_object()
        {
            var theService = new ColorService("red");

            var container = new Container(x =>
            {
                x.For<IService>().Use(theService);
                x.For<IGateway>().Use<DefaultGateway>();

                x.ForConcreteType<ClassWithNamedProperties>().Configure.Setter<int>().Is(5);

                x.Policies.SetAllProperties(
                    policy => policy.WithAnyTypeFromNamespace("StructureMap.Testing.Widget3"));
            });

            var description = container.Model.For<ClassWithNamedProperties>().Default.DescribeBuildPlan();
            Debug.WriteLine(description);

            var target = container.GetInstance<ClassWithNamedProperties>();
            target.Service.ShouldBeSameAs(theService);
            target.Gateway.ShouldBeOfType<DefaultGateway>();
        }

        // ENDSAMPLE

        [Fact]
        public void specify_setter_policy_by_of_type_and_construct_an_object()
        {
            var theService = new ColorService("red");

            var container = new Container(x =>
            {
                x.For<IService>().Use(theService);
                x.For<IGateway>().Use<DefaultGateway>();

                x.ForConcreteType<ClassWithNamedProperties>().Configure.Setter<int>().Is(5);

                x.Policies.SetAllProperties(policy => policy.OfType<IService>());
            });

            var description = container.Model.For<ClassWithNamedProperties>().Default.DescribeBuildPlan();
            Debug.WriteLine(description);

            var target = container.GetInstance<ClassWithNamedProperties>();
            target.Service.ShouldBeSameAs(theService);
            target.Gateway.ShouldBeNull();
        }

        [Fact]
        public void specify_setter_policy_by_a_predicate_on_property_type()
        {
            var theService = new ColorService("red");

            var container = new Container(x =>
            {
                x.For<IService>().Use(theService);
                x.For<IGateway>().Use<DefaultGateway>();

                x.Policies.SetAllProperties(policy => { policy.TypeMatches(type => type == typeof(IService)); });
            });

            var target = container.GetInstance<ClassWithNamedProperties>();
            target.Service.ShouldBeSameAs(theService);
            target.Gateway.ShouldBeNull();
        }
    }
    
    
}