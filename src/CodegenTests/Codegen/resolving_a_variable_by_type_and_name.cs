using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Frames;
using JasperFx.CodeGeneration.Model;
using Shouldly;
using Xunit;

namespace CodegenTests.Codegen;

internal static class GeneratedMethodExtensions
{
    internal static MethodFrameArranger ToArranger(this GeneratedMethod method)
    {
        return new MethodFrameArranger(method,
            new GeneratedType(new GenerationRules("SomeNamespace"), "SomeClassName"));
    }
}

public class resolving_a_variable_by_type_and_name
{
    [Fact]
    public void matches_one_of_the_arguments()
    {
        var arg1 = new Argument(typeof(string), "foo");
        var arg2 = new Argument(typeof(string), "bar");

        var frame1 = new FrameThatBuildsVariable("aaa", typeof(string));

        var method = new GeneratedMethod("Something", typeof(Task), new[] { arg1, arg2 });

        method.ToArranger().FindVariableByName(typeof(string), "foo")
            .ShouldBeSameAs(arg1);

        method.ToArranger().FindVariableByName(typeof(string), "bar")
            .ShouldBeSameAs(arg2);
    }


    [Fact]
    public void created_by_one_of_the_frames()
    {
        var arg1 = new Argument(typeof(string), "foo");
        var arg2 = new Argument(typeof(string), "bar");

        var frame1 = new FrameThatBuildsVariable("aaa", typeof(string));
        var frame2 = new FrameThatBuildsVariable("bbb", typeof(string));

        var method = new GeneratedMethod("Something", typeof(Task), arg1, arg2);
        method.Frames.Append(frame1, frame2);


        method.ToArranger().FindVariableByName(typeof(string), "aaa")
            .ShouldBeSameAs(frame1.Variable);

        method.ToArranger().FindVariableByName(typeof(string), "bbb")
            .ShouldBeSameAs(frame2.Variable);
    }

    [Fact]
    public void sourced_from_a_variable_source()
    {
        var arg1 = new Argument(typeof(string), "foo");
        var arg2 = new Argument(typeof(string), "bar");

        var frame1 = new FrameThatBuildsVariable("aaa", typeof(string));
        var frame2 = new FrameThatBuildsVariable("bbb", typeof(string));

        var method = new GeneratedMethod("Something", typeof(Task), new[] { arg1, arg2 });
        var source1 = new StubbedSource(typeof(string), "ccc");
        var source2 = new StubbedSource(typeof(string), "ddd");

        method.Sources.Add(source1);
        method.Sources.Add(source2);

        method.ToArranger().FindVariableByName(typeof(string), "ccc")
            .ShouldBeSameAs(source1.Variable);

        method.ToArranger().FindVariableByName(typeof(string), "ddd")
            .ShouldBeSameAs(source2.Variable);
    }

    [Fact]
    public void sad_path()
    {
        var arg1 = new Argument(typeof(string), "foo");
        var arg2 = new Argument(typeof(string), "bar");

        var frame1 = new FrameThatBuildsVariable("aaa", typeof(string));
        var frame2 = new FrameThatBuildsVariable("bbb", typeof(string));

        var method = new GeneratedMethod("Something", typeof(Task), new[] { arg1, arg2 });
        var source1 = new StubbedSource(typeof(string), "ccc");
        var source2 = new StubbedSource(typeof(string), "ddd");

        method.Sources.Add(source1);
        method.Sources.Add(source2);

        Exception<UnResolvableVariableException>.ShouldBeThrownBy(() =>
        {
            method.ToArranger().FindVariableByName(typeof(string), "missing");
        });
    }

    [Fact]
    public void sad_path_2()
    {
        var arg1 = new Argument(typeof(string), "foo");
        var arg2 = new Argument(typeof(string), "bar");

        var frame1 = new FrameThatBuildsVariable("aaa", typeof(string));
        var frame2 = new FrameThatBuildsVariable("bbb", typeof(string));

        var method = new GeneratedMethod("Something", typeof(Task), new[] { arg1, arg2 });
        var source1 = new StubbedSource(typeof(string), "ccc");
        var source2 = new StubbedSource(typeof(string), "ddd");

        method.Sources.Add(source1);
        method.Sources.Add(source2);

        Exception<UnResolvableVariableException>.ShouldBeThrownBy(() =>
        {
            method.ToArranger().FindVariableByName(typeof(int), "ccc");
        });
    }
}

public class StubbedSource : IVariableSource
{
    public readonly Variable Variable;

    public StubbedSource(Type dependencyType, string name)
    {
        Variable = new Variable(dependencyType, name);
    }

    public bool Matches(Type type)
    {
        return type == Variable.VariableType;
    }

    public Variable Create(Type type)
    {
        return Variable;
    }
}

public class FrameThatNeedsVariable : Frame
{
    private readonly Type _dependency;
    private readonly string _name;

    public FrameThatNeedsVariable(string name, Type dependency) : base(false)
    {
        _name = name;
        _dependency = dependency;
    }

    public Variable Resolved { get; private set; }

    public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
    {
    }

    public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
    {
        Resolved = chain.FindVariableByName(_dependency, _name);
        yield return Resolved;
    }
}

public class FrameThatBuildsVariable : Frame
{
    public readonly Variable Variable;

    public FrameThatBuildsVariable(string name, Type dependency) : base(false)
    {
        Variable = new Variable(dependency, name);
    }

    public override IEnumerable<Variable> Creates => new[] { Variable };

    public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
    {
        writer.WriteLine("FrameThatBuildsVariable");
    }
}