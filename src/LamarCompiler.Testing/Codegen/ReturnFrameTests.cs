using System.Linq;
using LamarCompiler.Frames;
using LamarCompiler.Scenarios;
using Shouldly;
using Xunit;

namespace LamarCompiler.Testing.Codegen
{
    public class ReturnFrameTests
    {
        [Fact]
        public void simple_use_case_no_value()
        {
            var result = CodegenScenario.ForBaseOf<ISimpleAction>(m => m.Frames.Add(new ReturnFrame()));
            
            result.LinesOfCode.ShouldContain("return;");
        }
        
        [Fact]
        public void return_a_variable_by_type()
        {
            var result = CodegenScenario.ForAction<int, int>(m => m.Return(typeof(int)));
            
            result.LinesOfCode.ShouldContain("return arg1;");
            result.Object.Create(5).ShouldBe(5);
        }
        
        [Fact]
        public void return_explicit_variable()
        {
            var result = CodegenScenario.ForAction<int, int>(m =>
            {
                var arg = m.Arguments.Single();
                m.Return(arg);
            });
            
            result.LinesOfCode.ShouldContain("return arg1;");
            result.Object.Create(5).ShouldBe(5);
        }
    }

    public interface ISimpleAction
    {
        void Go();
    }
}