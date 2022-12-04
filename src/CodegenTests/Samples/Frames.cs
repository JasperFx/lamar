using System;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Frames;
using JasperFx.CodeGeneration.Model;
using Xunit;
using Xunit.Abstractions;

namespace CodegenTests.Samples;

public class Frames
{
    private readonly ITestOutputHelper _output;

    public Frames(ITestOutputHelper output)
    {
        _output = output;
    }


    [Fact]
    public void write_method()
    {
        #region sample_write-new-method

        // Configures the code generation rules
        // and policies
        var rules = new GenerationRules("GeneratedNamespace");

        // Adds the "now : DateTime" variable rule to 
        // our generated code
        rules.Sources.Add(new NowTimeVariableSource());

        // Start the definition for a new generated assembly
        var assembly = new GeneratedAssembly(rules);

        // Add a new generated type called "WhatTimeIsIt" that will
        // implement the 
        var type = assembly.AddType("WhatTimeIsIt", typeof(ISaySomething));

        // Getting the definition for the method named "Speak"
        var method = type.MethodFor(nameof(ISaySomething.Speak));

        // Adding a frame that calls the NowSpeaker.Speak() method and
        // adding it to the generated method
        var call = new MethodCall(typeof(NowSpeaker), nameof(NowSpeaker.Speak));
        method.Frames.Add(call);

        // Compile the new code!
        assembly.CompileAll();

        #endregion


        // Write the generated code to the console here
        _output.WriteLine(type.SourceCode);
    }
}

#region sample_NowSpeaker

public static class NowSpeaker
{
    public static void Speak(DateTime now)
    {
        Console.WriteLine(now.ToString("o"));
    }
}

#endregion

#region sample_ISaySomething

public interface ISaySomething
{
    void Speak();
}

#endregion