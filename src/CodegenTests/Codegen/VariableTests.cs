using System.Collections.Generic;
using JasperFx.CodeGeneration.Model;
using Shouldly;
using Xunit;

namespace CodegenTests.Codegen;

public class VariableTests
{
    [Fact]
    public void override_the_name()
    {
        Variable variable = Variable.For<HyperdriveMotivator>();
        variable.OverrideName("thing");

        variable.Usage.ShouldBe("thing");
    }

    [Fact]
    public void default_arg_name_of_normal_class()
    {
        Variable.DefaultArgName<HyperdriveMotivator>()
            .ShouldBe("hyperdriveMotivator");
    }

    [Fact]
    public void default_arg_name_of_closed_interface()
    {
        Variable.DefaultArgName<IHyperdriveMotivator>()
            .ShouldBe("hyperdriveMotivator");
    }

    [Fact]
    public void default_arg_name_of_array()
    {
        Variable.DefaultArgName<IWidget[]>()
            .ShouldBe("widgetArray");
    }

    [Fact]
    public void default_arg_name_of_List()
    {
        Variable.DefaultArgName<IList<IWidget>>()
            .ShouldBe("widgetIList");

        Variable.DefaultArgName<List<IWidget>>()
            .ShouldBe("widgetList");

        Variable.DefaultArgName<IReadOnlyList<IWidget>>()
            .ShouldBe("widgetIReadOnlyList");
    }

    [Fact]
    public void default_arg_name_of_Collection()
    {
        Variable.DefaultArgName<ICollection<IWidget>>()
            .ShouldBe("widgetICollection");

        Variable.DefaultArgName<IReadOnlyCollection<IWidget>>()
            .ShouldBe("widgetIReadOnlyCollection");
    }

    [Fact]
    public void default_arg_name_of_enumerable()
    {
        Variable.DefaultArgName<IEnumerable<IWidget>>()
            .ShouldBe("widgetIEnumerable");
    }

    [Fact]
    public void default_arg_name_of_generic_class_with_single_parameter()
    {
        Variable.DefaultArgName<FooHandler<HyperdriveMotivator>>()
            .ShouldBe("fooHandler");
    }

    [Fact]
    public void default_arg_name_of_generic_interface_with_single_parameter()
    {
        Variable.DefaultArgName<IFooHandler<HyperdriveMotivator>>()
            .ShouldBe("fooHandler");
    }

    [Fact]
    public void default_arg_name_of_open_generic_type()
    {
        Variable.DefaultArgName(typeof(IOpenGeneric<>))
            .ShouldBe("openGeneric");

        Variable.DefaultArgName(typeof(FooHandler<>)).ShouldBe("fooHandler");
    }

    [Fact]
    public void default_arg_name_of_inner_class()
    {
        Variable.DefaultArgName<HyperdriveMotivator.InnerThing>()
            .ShouldBe("innerThing");
    }

    [Fact]
    public void default_arg_name_of_inner_interface()
    {
        Variable.DefaultArgName<HyperdriveMotivator.IInnerThing>()
            .ShouldBe("innerThing");
    }
}

public interface IWidget
{
}

public class FooHandler<T>
{
}

public interface IOpenGeneric<T>
{
}

public interface IFooHandler<T>
{
}

public interface IHyperdriveMotivator
{
}

public class HyperdriveMotivator
{
    public class InnerThing
    {
    }

    public interface IInnerThing
    {
    }
}