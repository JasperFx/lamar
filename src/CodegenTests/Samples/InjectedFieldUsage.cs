using System;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Frames;
using JasperFx.CodeGeneration.Model;
using Xunit;
using Xunit.Abstractions;

namespace CodegenTests.Samples;

public class InjectedFieldUsage
{
    private readonly ITestOutputHelper _output;

    public InjectedFieldUsage(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void generate_a_class()
    {
        #region sample_using-injected-field

        var assembly = GeneratedAssembly.Empty();
        var type = assembly.AddType("WhatTimeIsIt", typeof(ISaySomething));

        var method = type.MethodFor(nameof(ISaySomething.Speak));

        var call = new MethodCall(typeof(NowSpeaker), nameof(NowSpeaker.Speak));

        // Create an InjectedField as the argument to
        // the Speak method
        var now = new InjectedField(typeof(DateTime), "now");
        call.Arguments[0] = now;

        method.Frames.Add(call);

        assembly.CompileAll();

        #endregion


        _output.WriteLine(type.SourceCode);
    }
}