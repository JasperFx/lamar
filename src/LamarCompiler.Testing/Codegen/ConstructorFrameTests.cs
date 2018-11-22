using System;
using LamarCompiler.Frames;
using LamarCompiler.Scenarios;
using Shouldly;
using Xunit;

namespace LamarCompiler.Testing.Codegen
{
    public class ConstructorFrameTests
    {
        public class NoArgGuy : IDisposable
        {
            public NoArgGuy()
            {
            }

            public void Dispose()
            {
                WasDisposed = true;
            }

            public bool WasDisposed { get; set; }
            
            public int Number { get; set; }
            public double Double { get; set; }
            public string String { get; set; }
        }

        public class NoArgGuyCatcher
        {
            public void Catch(NoArgGuy guy)
            {
                Guy = guy;
            }

            public NoArgGuy Guy { get; set; }
        }
        
        [Fact]
        public void no_arg_no_return_no_using_simplest_case()
        {
            var result = CodegenScenario.ForBuilds<NoArgGuy>(m =>
            {
                m.CallConstructor(() => new NoArgGuy());
                m.Return(typeof(NoArgGuy));
            });
            
            result.LinesOfCode.ShouldContain($"var noArgGuy = new {typeof(NoArgGuy).FullNameInCode()}();");
            result.Object.Build().ShouldNotBeNull();
        }
        
        [Fact]
        public void no_arg_inside_of_using_block_simplest_case()
        {
            var result = CodegenScenario.ForAction<NoArgGuyCatcher>(m =>
            {
                m.CallConstructor(() => new NoArgGuy()).Mode = ConstructorCallMode.UsingNestedVariable;
                m.Call<NoArgGuyCatcher>(x => x.Catch(null));
            });
            
            result.LinesOfCode.ShouldContain($"using (var noArgGuy = new {typeof(NoArgGuy).FullNameInCode()}())");
            
            var catcher = new NoArgGuyCatcher();
            result.Object.DoStuff(catcher);
            
            catcher.Guy.ShouldNotBeNull();
            catcher.Guy.WasDisposed.ShouldBeTrue();
        }
        
        [Fact]
        public void no_arg_return_as_built_simplest_case()
        {
            var result = CodegenScenario.ForBuilds<NoArgGuy>(m =>
            {
                m.CallConstructor(() => new NoArgGuy()).Mode = ConstructorCallMode.ReturnValue;
            });
            
            result.LinesOfCode.ShouldContain($"return new {typeof(NoArgGuy).FullNameInCode()}();");
            result.Object.Build().ShouldNotBeNull();
        }
        
        [Fact]
        public void no_arg_return_with_one_setter_case()
        {
            var result = CodegenScenario.ForBuilds<NoArgGuy, int>(m =>
            {
                var @call = m.CallConstructor(() => new NoArgGuy());
                @call.Mode = ConstructorCallMode.ReturnValue;
                @call.Set(x => x.Number);
            });
            
            result.Object.Create(11).Number.ShouldBe(11);
        }
        
        [Fact]
        public void no_arg_return_with_two_setters_case()
        {
            var result = CodegenScenario.ForBuilds<NoArgGuy, int, double>(m =>
            {
                var @call = m.CallConstructor(() => new NoArgGuy());
                @call.Mode = ConstructorCallMode.ReturnValue;
                @call.Set(x => x.Number);
                @call.Set(x => x.Double);
            });

            var noArgGuy = result.Object.Create(11, 1.22);
            noArgGuy.Number.ShouldBe(11);
            noArgGuy.Double.ShouldBe(1.22);
        }
        
        [Fact]
        public void no_arg_return_with_three_setters_case()
        {
            var result = CodegenScenario.ForBuilds<NoArgGuy, int, double, string>(m =>
            {
                var @call = m.CallConstructor(() => new NoArgGuy());
                @call.Mode = ConstructorCallMode.ReturnValue;
                @call.Set(x => x.Number);
                @call.Set(x => x.Double);
                @call.Set(x => x.String);
            });

            var noArgGuy = result.Object.Create(11, 1.22, "wow");
            noArgGuy.Number.ShouldBe(11);
            noArgGuy.Double.ShouldBe(1.22);
            noArgGuy.String.ShouldBe("wow");
        }
        
        /*
         * Test cases

         * 4. No args, one setter
         * 5. No args, multiple setters
         * 6. No args, extra frame, NOT return
         * 7. No args, extra frame, return
         * 8. No args, extra frame, using 
         * 9. No args, multiple setters, extra frame, NOT return
         * 10. No args, multiple setters, extra frame, using 
         * 11. Specify some arguments
         * 12. Multiple arguments
         * 13. Explicit declared type
         *
         * 
         */
    }
}