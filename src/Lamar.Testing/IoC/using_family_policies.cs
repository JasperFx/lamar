using System;
using Lamar.IoC.Enumerables;
using Lamar.IoC.Instances;
using Lamar.IoC.Lazy;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC
{
    public class using_family_policies
    {
        [Fact]
        public void default_policies_in_empty_graph()
        {
            var graph = ServiceGraph.Empty();

            
            graph.FamilyPolicies[0].ShouldBeOfType<EnumerablePolicy>();
            graph.FamilyPolicies[1].ShouldBeOfType<FuncOrLazyPolicy>();
            
            graph.FamilyPolicies[2].ShouldBeOfType<CloseGenericFamilyPolicy>();
            graph.FamilyPolicies[3].ShouldBeOfType<ConcreteFamilyPolicy>();
            graph.FamilyPolicies[4].ShouldBeOfType<EmptyFamilyPolicy>();

        }
        
        [Fact]
        public void custom_policies_take_precedence()
        {
            var graph = ServiceGraph.For(_ =>
            {
                _.Policies.OnMissingFamily<CustomMissingFamily>();
            });
            
            graph.FamilyPolicies[0].ShouldBeOfType<CustomMissingFamily>();
            
            graph.FamilyPolicies[1].ShouldBeOfType<EnumerablePolicy>();
            graph.FamilyPolicies[2].ShouldBeOfType<FuncOrLazyPolicy>();
            graph.FamilyPolicies[3].ShouldBeOfType<CloseGenericFamilyPolicy>();
            graph.FamilyPolicies[4].ShouldBeOfType<ConcreteFamilyPolicy>();
            graph.FamilyPolicies[5].ShouldBeOfType<EmptyFamilyPolicy>();
        }
        
        [Fact]
        public void use_custom_policy()
        {
            var graph = ServiceGraph.For(_ =>
            {
                _.Policies.OnMissingFamily<CustomMissingFamily>();
            });
            
            graph.TryToCreateMissingFamily(typeof(FakeThing))
                .Default.ShouldBeOfType<ObjectInstance>()
                .Service.ShouldBeOfType<FakeThing>()
                .CreatedBy.ShouldBe("CustomMissingFamily");
        }
        
        [Fact]
        public void use_end_to_end()
        {
            var container = Container.For(_ =>
            {
                _.Policies.OnMissingFamily<CustomMissingFamily>();
            });
            
            container.GetInstance<FakeThing>().CreatedBy.ShouldBe("CustomMissingFamily");
        }
        
        [Fact]
        public void use_end_to_end_with_get_by_name()
        {
            var container = Container.For(_ =>
            {
                _.Policies.OnMissingFamily<ColorPolicy>();
            });
            
            container.GetInstance<Color>("Red").Name.ShouldBe("Red");
            container.GetInstance<Color>("Blue").Name.ShouldBe("Blue");
            
            
        }
        
        [Fact]
        public void pick_up_concrete_type_with_a_fillable_constructor()
        {
            var container = Container.For(_ =>
            {
                _.AddTransient<IWidget, AWidget>();
                _.AddTransient<IClock, Clock>();
            });
            container.GetInstance<FakeThing>().ShouldNotBeNull();

            var user = container.GetInstance<WidgetUser>();
            user.Clock.ShouldBeOfType<Clock>();
            user.Widget.ShouldBeOfType<AWidget>();
        }
        
        public class TestClass{}
        
        [Fact]
        public void do_not_pick_up_concrete_type_with_no_usable_ctor()
        {
            var graph = ServiceGraph.For(_ =>
            {
                //_.AddTransient<IWidget, AWidget>();
                //_.AddTransient<IClock, Clock>();
            });
            
            graph.TryToCreateMissingFamily(typeof(WidgetUser))
                .Default.ShouldBeNull();
        }
        
        [Fact]
        public void can_pick_up_concrete_type_with_no_usable_ctor()
        {
            var graph = ServiceGraph.For(_ =>
            {
                _.AddTransient<IWidget, AWidget>();
                _.AddTransient<IClock, Clock>();
            });
            
            graph.TryToCreateMissingFamily(typeof(WidgetUser))
                .Default.ShouldBeOfType<ConstructorInstance>()
                .ImplementationType.ShouldBe(typeof(WidgetUser));
        }
        
        [Fact]
        public void recursive_family_creation()
        {
            #region sample_register-ColorPolicy
            var container = Container.For(_ =>
            {
                _.Policies.OnMissingFamily<ColorPolicy>();
            });
            #endregion
            
            container.GetInstance<ColorUser>()
                .Color.ShouldNotBeNull();
        }

        

    }

    
    #region sample_Color
    public class Color
    {
        public string Name { get; set; }
    }
    #endregion
    
    
    public class ColorUser
    {
        public Color Color { get; }

        public ColorUser(Color color)
        {
            Color = color;
        }
    }


    #region sample_ColorPolicy
    public class ColorPolicy : IFamilyPolicy
    {
        public ServiceFamily Build(Type type, ServiceGraph serviceGraph)
        {
            if (type != typeof(Color)) return null;
            
            return new ServiceFamily(type, serviceGraph.DecoratorPolicies, 
                ObjectInstance.For(new Color{Name = "Red"}).Named("Red"),
                ObjectInstance.For(new Color{Name = "Blue"}).Named("Blue"),
                ObjectInstance.For(new Color{Name = "Green"}).Named("Green")
                
                
                );
        }
    }
    #endregion

    public class FakeThing
    {
        public string CreatedBy { get; set; }
    }
    
    public class CustomMissingFamily : IFamilyPolicy
    {
        public ServiceFamily Build(Type type, ServiceGraph serviceGraph)
        {
            if (type == typeof(FakeThing))
            {
                var @default = new ObjectInstance(type, new FakeThing{CreatedBy = "CustomMissingFamily"});
                return new ServiceFamily(type, serviceGraph.DecoratorPolicies, @default);
            }

            return null;
        }
    }
}