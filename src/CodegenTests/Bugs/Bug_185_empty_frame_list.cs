using System;
using JasperFx.CodeGeneration;
using JasperFx.Core.Reflection;
using JasperFx.RuntimeCompiler;
using Shouldly;
using Xunit;

namespace CodegenTests.Bugs;

public class Bug_185_empty_frame_list
{
    [Fact]
    public void GenerateControllerAssembly()
    {
        var rules = new GenerationRules();

        var assembly = new GeneratedAssembly(rules);

        var testController = assembly.AddType("TestController", typeof(TestControllerBase));


        new AssemblyGenerator().Compile(assembly);
        var controllerInstance =
            Activator.CreateInstance(testController.CompiledType).As<TestControllerBase>();
        controllerInstance.Get().ShouldBe("Got");
    }

    public class TestControllerBase
    {
        public virtual string Get()
        {
            return "Got";
        }
    }
}