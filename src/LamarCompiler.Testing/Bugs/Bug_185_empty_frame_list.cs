using System;
using Baseline;
using LamarCodeGeneration;
using LamarCompiler;
using Shouldly;
using Xunit;

namespace Lamar.Testing.Bugs
{
    public class Bug_185_empty_frame_list
    {
        public class TestControllerBase
        { 
            public virtual string Get() { return "Got"; }
        }

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
    }
}