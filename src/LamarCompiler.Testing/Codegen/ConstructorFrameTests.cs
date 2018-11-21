using LamarCompiler.Scenarios;
using Xunit;

namespace LamarCompiler.Testing.Codegen
{
    public class ConstructorFrameTests
    {
        public class NoArgGuy
        {
            public NoArgGuy()
            {
            }
        }
        
        [Fact]
        public void no_arg_no_return_no_using_simplest_case()
        {
//            var result = CodegenScenario.ForBuilds<NoArgGuy>(m =>
//            {
//                m.
//            })
        }
        
        /*
         * Test cases
         * 1. No arg constructor, nothing else
         * 2. No args, using
         * 3. No args, return
         * 4. No args, one setter
         * 5. No args, multiple setters
         * 6. No args, extra frame, NOT return
         * 7. No args, extra frame, return
         * 8. No args, extra frame, using 
         * 9. No args, multiple setters, extra frame, NOT return
         * 10. No args, multiple setters, extra frame, using 
         * 11. Specify some arguments
         *
         *
         *
         * 
         */
    }
}