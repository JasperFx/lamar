using System.Linq;
using System.Threading.Tasks;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Model;
using Shouldly;
using Xunit;

namespace CodegenTests.Codegen;

public class GeneratedType_automatically_adds_methods_for_base_types_and_interfaces
{
    [Fact]
    public void generate_methods_for_an_interface()
    {
        var generatedType = new GeneratedType("Foo").Implements<IHasMethods>();

        generatedType.Methods.Count().ShouldBe(3);

        generatedType.MethodFor("DoStuff").ShouldNotBeNull();
        generatedType.MethodFor("SayStuff").ShouldNotBeNull();
        generatedType.MethodFor("AddNumbers").ShouldNotBeNull();
    }

    [Fact]
    public void determines_arguments_from_method_signature()
    {
        var generatedType = new GeneratedType("Foo").Implements<IHasMethods>();
        generatedType.MethodFor("DoStuff").Arguments.Any().ShouldBeFalse();
        generatedType.MethodFor("SayStuff").Arguments.Single().ShouldBe(Argument.For<string>("name"));

        generatedType.MethodFor("AddNumbers").Arguments
            .ShouldBe(new[] { Argument.For<int>("x"), Argument.For<int>("y") });
    }

    [Fact]
    public void generate_method_for_void_signature()
    {
        var generatedType = new GeneratedType("Foo").Implements<IHasMethods>();
        generatedType.MethodFor("DoStuff").ReturnType.ShouldBe(typeof(void));
    }

    [Fact]
    public void generate_method_for_single_return_value()
    {
        var generatedType = new GeneratedType("Foo").Implements<IHasMethods>();
        generatedType.MethodFor("AddNumbers").ReturnType.ShouldBe(typeof(int));
    }

    [Fact]
    public void generate_method_for_return_type_of_Task()
    {
        var generatedType = new GeneratedType("Foo").Implements<IHasTaskMethods>();
        generatedType.MethodFor("DoStuff").ReturnType.ShouldBe(typeof(Task));
    }

    [Fact]
    public void generate_method_for_Task_of_value_method()
    {
        var generatedType = new GeneratedType("Foo").Implements<IHasTaskMethods>();
        generatedType.MethodFor("AddNumbers").ReturnType.ShouldBe(typeof(Task<int>));
    }

    [Fact]
    public void pick_up_methods_on_base_class()
    {
        var generatedType = new GeneratedType("Foo").InheritsFrom<BaseClassWithMethods>();

        generatedType.Methods.Select(x => x.MethodName)
            .ShouldBe(new[] { "Go2", "Go3", "Go5", "Go6" });
    }

    [Fact]
    public void all_methods_in_base_class_should_be_override()
    {
        var generatedType = new GeneratedType("Foo").InheritsFrom<BaseClassWithMethods>();
        foreach (var method in generatedType.Methods) method.Overrides.ShouldBeTrue();
    }

    [Fact]
    public void all_methods_from_interface_should_not_be_overrides()
    {
        var generatedType = new GeneratedType("Foo").Implements<IHasTaskMethods>();
        foreach (var method in generatedType.Methods) method.Overrides.ShouldBeFalse();
    }
}

public abstract class BaseClassWithMethods
{
    public void Go()
    {
    }

    public virtual void Go2()
    {
    }

    public abstract void Go3();

    public void Go4()
    {
    }

    public virtual void Go5()
    {
    }

    public abstract void Go6();
}

public interface IHasMethods
{
    void DoStuff();
    void SayStuff(string name);
    int AddNumbers(int x, int y);
}

public interface IHasTaskMethods
{
    Task DoStuff(string name);

    Task<int> AddNumbers(int x, int y);
}