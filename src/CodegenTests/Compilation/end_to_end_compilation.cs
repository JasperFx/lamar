using System;
using System.Collections.Generic;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Frames;
using JasperFx.CodeGeneration.Model;
using JasperFx.Core.Reflection;
using Shouldly;
using Xunit;

namespace CodegenTests.Compilation;

public class end_to_end_compilation
{
    [Fact]
    public void generate_dynamic_types_with_no_fields()
    {
        var rules = new GenerationRules("Lamar.Compilation");
        var assembly = new GeneratedAssembly(rules);


        var adder = assembly.AddType("Adder", typeof(INumberGenerator));
        adder.MethodFor("Generate").Frames.Append<AddFrame>();

        var multiplier = assembly.AddType("Multiplier", typeof(INumberGenerator));
        multiplier.MethodFor(nameof(INumberGenerator.Generate))
            .Frames.Append<MultiplyFrame>();

        assembly.CompileAll();

        Activator.CreateInstance(adder.CompiledType)
            .As<INumberGenerator>()
            .Generate(3, 4).ShouldBe(7);

        Activator.CreateInstance(multiplier.CompiledType)
            .As<INumberGenerator>()
            .Generate(3, 4).ShouldBe(12);

        adder.SourceCode.ShouldContain("public class Adder");
        multiplier.SourceCode.ShouldContain("public class Multiplier");
    }
}

public class AddFrame : SyncFrame
{
    private Variable _one;
    private Variable _two;

    public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
    {
        writer.WriteLine($"return {_one.Usage} + {_two.Usage};");
    }

    public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
    {
        _one = chain.FindVariableByName(typeof(int), "one");
        yield return _one;

        _two = chain.FindVariableByName(typeof(int), "two");
        yield return _two;
    }
}

public class MultiplyFrame : SyncFrame
{
    private Variable _one;
    private Variable _two;

    public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
    {
        writer.WriteLine($"return {_one.Usage} * {_two.Usage};");
    }

    public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
    {
        _one = chain.FindVariableByName(typeof(int), "one");
        yield return _one;

        _two = chain.FindVariableByName(typeof(int), "two");
        yield return _two;
    }
}

public interface INumberGenerator
{
    int Generate(int one, int two);
}