using System.Collections.Generic;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Frames;
using JasperFx.CodeGeneration.Model;
using JasperFx.RuntimeCompiler.Scenarios;
using Shouldly;
using Xunit;

namespace CodegenTests.Codegen.Scenarios;

public class basic_execution
{
    [Fact]
    public void simple_execution_with_action()
    {
        var result = CodegenScenario.ForAction<Tracer>((t, m) => m.Frames.Call<Tracer>(x => x.Call()));

        var tracer = new Tracer();
        result.Object.DoStuff(tracer);

        tracer.Called.ShouldBeTrue();

        result.LinesOfCode.ShouldContain("arg1.Call();");
    }

    [Fact]
    public void simple_execution_with_action_2()
    {
        var result = CodegenScenario.ForAction<Tracer>(m => m.Frames.Call<Tracer>(x => x.Call()));

        var tracer = new Tracer();
        result.Object.DoStuff(tracer);

        tracer.Called.ShouldBeTrue();

        result.LinesOfCode.ShouldContain("arg1.Call();");
    }

    [Fact]
    public void simple_execution_with_one_input_and_output()
    {
        var result = CodegenScenario.ForBuilds<int, int>((t, m) => m.Frames.Append<AddTwoFrame>());

        result.Object.Create(5).ShouldBe(7);
    }

    [Fact]
    public void simple_execution_with_one_input_and_output_2()
    {
        var result = CodegenScenario.ForBuilds<int, int>(m => m.Frames.Append<AddTwoFrame>());

        result.Object.Create(5).ShouldBe(7);
    }
}

public class Tracer
{
    public bool Called { get; set; }

    public void Call()
    {
        Called = true;
    }
}

public class AddTwoFrame : SyncFrame
{
    private Variable _number;

    public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
    {
        writer.Write($"return {_number.Usage} + 2;");
    }

    public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
    {
        _number = chain.FindVariable(typeof(int));
        yield return _number;
    }
}