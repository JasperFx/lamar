using Shouldly;
using StructureMap.Testing.Widget;
using StructureMap.Testing.Widget2;
using StructureMap.Testing.Widget3;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance;

public class inline_dependencies
{
    [Fact]
    public void use_null_as_inline_dependency()
    {
        var container = new Container(_ =>
        {
            _.For<IWidget>().Use<RedWidget>().Named("Red");
            _.For<IWidget>().Use<BlueWidget>().Named("Blue");

            _.ForConcreteType<ClassWithWidget>().Configure.Ctor<IWidget>().IsNull();
        });

        container.GetInstance<ClassWithWidget>()
            .Widget.ShouldBeNull();
    }

    [Fact]
    public void use_a_referenced_by_name_dependency()
    {
        var container = new Container(_ =>
        {
            _.For<IWidget>().Use<RedWidget>().Named("Red");
            _.For<IWidget>().Use<BlueWidget>().Named("Blue");

            _.ForConcreteType<ClassWithWidget>().Configure.Ctor<IWidget>().IsNamedInstance("Red");
        });

        container.GetInstance<ClassWithWidget>()
            .Widget.ShouldBeOfType<RedWidget>();
    }

    [Fact]
    public void specify_ctorarg_with_non_simple_argument()
    {
        var widget = new ColorWidget("Red");
        var container = new Container(x => x.For<ClassWithWidget>()
            .Use<ClassWithWidget>()
            .Ctor<IWidget>().Is(widget));

        widget.ShouldBeSameAs(container.GetInstance<ClassWithWidget>().Widget);
    }

    [Fact]
    public void specify_ctorarg_with_non_simple_argument_by_type()
    {
        var container = new Container(x => x.For<ClassWithWidget>()
            .Use<ClassWithWidget>()
            .Ctor<IWidget>().Is<BlueWidget>());

        container.GetInstance<ClassWithWidget>()
            .Widget.ShouldBeOfType<BlueWidget>();
    }

    [Fact]
    public void specify_ctorarg_with_non_simple_argument_as_singleton()
    {
        var widget = new ColorWidget("Red");
        var container = new Container(x => x.For<ClassWithWidget>()
            .Use<ClassWithWidget>()
            .Singleton()
            .Ctor<IWidget>().Is(widget));

        widget.ShouldBeSameAs(container.GetInstance<ClassWithWidget>().Widget);
    }


    [Fact]
    public void inline_usage_of_primitive_constructor_argument()
    {
        var container = new Container(_ =>
        {
            _.For<IWidget>().Use<ColorWidget>()
                .Ctor<string>().Is("Red");
        });

        container.GetInstance<IWidget>()
            .ShouldBeOfType<ColorWidget>()
            .Color.ShouldBe("Red");
    }

    [Fact]
    public void can_use_lambda_as_inline_dependency()
    {
        var container = new Container(x =>
        {
            x.ForConcreteType<DecoratedGateway>().Configure
                .Ctor<IGateway>().Is(c => new StubbedGateway());
        });

        container.GetInstance<DecoratedGateway>()
            .InnerGateway.ShouldBeOfType<StubbedGateway>();
    }

    [Fact]
    public void supply_named_arguments()
    {
        var container = new Container(x => { x.For<IWidget>().Use<ColorWidget>().Ctor<string>().Is("Red"); });

        container.GetInstance<IWidget>()
            .ShouldBeOfType<ColorWidget>()
            .Color.ShouldBe("Red");
    }

    [Fact]
    public void deep_graph()
    {
        var container = new Container(x =>
        {
            x.For<IParent>().Use<Parent>()
                .Ctor<string>("name").Is("Jerry")
                .Ctor<Child>().Is<Child>(child =>
                {
                    child.Ctor<string>("name").Is("Monte")
                        .Ctor<GrandChild>().Is<GrandChild>(grand => { grand.Ctor<string>("name").Is("Jeremy"); });
                });
        });

        var parent = container.GetInstance<IParent>()
            .ShouldBeOfType<Parent>();

        parent.Name.ShouldBe("Jerry");
        parent.Child.Name.ShouldBe("Monte");
        parent.Child.GrandChild.Name.ShouldBe("Jeremy");
    }


    [Fact]
    public void as_inline_dependency()
    {
        var container = new Container(x =>
        {
            // Build by a simple Expression<Func<T>>
            x.For<RuleHolder>()
                .Add<RuleHolder>()
                .Named("Red")
                .Ctor<Rule>().Is(c => new ColorRule("Red"));

            // Build by a simple Expression<Func<IBuildSession, T>>
            x.For<RuleHolder>()
                .Add<RuleHolder>()
                .Named("Blue").Ctor<Rule>().Is("The Blue One", c => new ColorRule("Blue"));

            // Build by Func<T> with a user supplied description
            x.For<RuleHolder>()
                .Add<RuleHolder>()
                .Named("Green")
                .Ctor<Rule>().Is("The Green One", s => s.GetInstance<RuleBuilder>().ForColor("Green"));

            // Build by Func<IBuildSession, T> with a user description
            x.For<RuleHolder>()
                .Add<RuleHolder>()
                .Named("Purple")
                .Ctor<Rule>()
                .Is("The Purple One", s => s.GetInstance<RuleBuilder>().ForColor("Purple"));
        });

        container.GetInstance<RuleHolder>("Red").Rule.ShouldBeOfType<ColorRule>().Color.ShouldBe("Red");
        container.GetInstance<RuleHolder>("Blue").Rule.ShouldBeOfType<ColorRule>().Color.ShouldBe("Blue");
        container.GetInstance<RuleHolder>("Green").Rule.ShouldBeOfType<ColorRule>().Color.ShouldBe("Green");
        container.GetInstance<RuleHolder>("Purple").Rule.ShouldBeOfType<ColorRule>().Color.ShouldBe("Purple");
    }

    public class ClassWithWidget
    {
        public ClassWithWidget(IWidget widget)
        {
            Widget = widget;
        }

        public IWidget Widget { get; }
    }

    public class GrandChild
    {
        public GrandChild(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public class Child
    {
        public Child(string name, GrandChild grandChild)
        {
            Name = name;
            GrandChild = grandChild;
        }

        public string Name { get; }

        public GrandChild GrandChild { get; }
    }

    public interface IParent
    {
        string Name { get; }
        Child Child { get; }
    }

    public class Parent : IParent
    {
        public Parent(string name, Child child)
        {
            Name = name;
            Child = child;
        }

        public string Name { get; }

        public Child Child { get; }
    }

    public class RuleHolder
    {
        public RuleHolder(Rule rule)
        {
            Rule = rule;
        }

        public Rule Rule { get; }
    }

    public class RuleBuilder
    {
        public Rule ForColor(string color)
        {
            return new ColorRule(color);
        }
    }
}

public class Bug_244_using_Func_of_string_as_a_dependency
{
    [Fact]
    public void use_a_simple_func_for_string_dependency()
    {
        var container = new Container(x => { x.For<Rule>().Use<ColorRule>().Ctor<string>().Is(c => "blue"); });

        container.GetInstance<Rule>()
            .ShouldBeOfType<ColorRule>().Color.ShouldBe("blue");
    }

    [Fact]
    public void use_a_func_of_context_for_string_dependency()
    {
        var container = new Container(x =>
        {
            x.ForConcreteType<ColorRule>().Configure.Ctor<string>().Is("fuschia");
            x.ForConcreteType<StringHolder>().Configure.Ctor<string>()
                .Is(c => c.GetInstance<ColorRule>().Color);
        });

        container.GetInstance<StringHolder>().Name.ShouldBe("fuschia");
    }

    [Fact]
    public void use_a_func_for_a_simple_type()
    {
        var container = new Container(x => { x.For<IntHolder>().Use<IntHolder>().Ctor<int>().Is(c => 5); });

        container.GetInstance<IntHolder>()
            .Number.ShouldBe(5);
    }

    [Fact]
    public void use_a_func_for_enums()
    {
        var container =
            new Container(
                x => { x.For<EnumHolder>().Use<EnumHolder>().Ctor<BreedEnum>().Is(c => BreedEnum.Beefmaster); });

        // My father raises Beefmasters and there'd be
        // hell to pay if he caught me using Angus as
        // test data
        container.GetInstance<EnumHolder>()
            .Breed.ShouldBe(BreedEnum.Beefmaster);
    }
}

public class EnumHolder
{
    public EnumHolder(BreedEnum breed)
    {
        Breed = breed;
    }

    public BreedEnum Breed { get; }
}

public class IntHolder
{
    public IntHolder(int number)
    {
        Number = number;
    }

    public int Number { get; }
}

public class StringHolder
{
    public StringHolder(string name)
    {
        Name = name;
    }

    public string Name { get; }
}

public class PrimitiveArrayTester
{
    [Fact]
    public void specify_a_string_array()
    {
        var container = new Container(x =>
        {
            x.ForConcreteType<ClassWithStringAndIntArray>().Configure
                .Ctor<string[]>().Is(new[] { "a", "b", "c" })
                .Ctor<int[]>().Is(new[] { 1, 2, 3 });
        });

        var objectWithArrays = container.GetInstance<ClassWithStringAndIntArray>();
        objectWithArrays.Numbers.ShouldBe(new[] { 1, 2, 3 });
        objectWithArrays.Strings.ShouldBe(new[] { "a", "b", "c" });
    }

    public class ClassWithStringAndIntArray
    {
        public ClassWithStringAndIntArray(int[] numbers, string[] strings)
        {
            Numbers = numbers;
            Strings = strings;
        }

        public int[] Numbers { get; }

        public string[] Strings { get; }
    }
}