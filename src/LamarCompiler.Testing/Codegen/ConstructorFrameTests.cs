using System;
using Lamar.IoC.Instances;
using LamarCompiler.Frames;
using LamarCompiler.Scenarios;
using Shouldly;
using Xunit;

namespace LamarCompiler.Testing.Codegen
{
    public class ConstructorFrameTests
    {
        public interface IGuy{}
        
        public class NoArgGuy : IGuy, IDisposable
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
                m.Frames.CallConstructor(() => new NoArgGuy());
                m.Frames.Return(typeof(NoArgGuy));
            });
            
            result.LinesOfCode.ShouldContain($"var noArgGuy = new {typeof(NoArgGuy).FullNameInCode()}();");
            result.Object.Build().ShouldNotBeNull();
        }
        
        [Fact]
        public void override_built_type()
        {
            var result = CodegenScenario.ForBuilds<IGuy>(m =>
            {
                m.Frames.CallConstructor(() => new NoArgGuy(), c => c.DeclaredType = typeof(IGuy));
                m.Frames.Return(typeof(NoArgGuy));
            });
            
            result.LinesOfCode.ShouldContain($"{typeof(IGuy).FullNameInCode()} noArgGuy = new {typeof(NoArgGuy).FullNameInCode()}();");
            result.Object.Build().ShouldNotBeNull();
        }
        
        [Fact]
        public void no_arg_inside_of_using_block_simplest_case()
        {
            var result = CodegenScenario.ForAction<NoArgGuyCatcher>(m =>
            {
                m.Frames.CallConstructor(() => new NoArgGuy(), c => c.Mode = ConstructorCallMode.UsingNestedVariable);
                m.Frames.Call<NoArgGuyCatcher>(x => x.Catch(null));
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
                m.Frames.CallConstructor(() => new NoArgGuy(), c => c.Mode = ConstructorCallMode.ReturnValue);
            });
            
            result.LinesOfCode.ShouldContain($"return new {typeof(NoArgGuy).FullNameInCode()}();");
            result.Object.Build().ShouldNotBeNull();
        }
        
        [Fact]
        public void no_arg_return_with_one_setter_case()
        {
            var result = CodegenScenario.ForBuilds<NoArgGuy, int>(m =>
            {
                m.Frames.CallConstructor(() => new NoArgGuy(), @call =>
                {
                    @call.Mode = ConstructorCallMode.ReturnValue;
                    @call.Set(x => x.Number);
                });

            });
            
            result.Object.Create(11).Number.ShouldBe(11);
        }
        
        [Fact]
        public void no_arg_return_with_two_setters_case()
        {
            var result = CodegenScenario.ForBuilds<NoArgGuy, int, double>(m =>
            {
                m.Frames.CallConstructor(() => new NoArgGuy(), @call =>
                {
                    @call.Mode = ConstructorCallMode.ReturnValue;
                    @call.Set(x => x.Number);
                    @call.Set(x => x.Double);
                });

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
                m.Frames.CallConstructor(() => new NoArgGuy(), @call =>
                {                
                    @call.Mode = ConstructorCallMode.ReturnValue;
                    @call.Set(x => x.Number);
                    @call.Set(x => x.Double);
                    @call.Set(x => x.String);
                    
                });

            });

            var noArgGuy = result.Object.Create(11, 1.22, "wow");
            noArgGuy.Number.ShouldBe(11);
            noArgGuy.Double.ShouldBe(1.22);
            noArgGuy.String.ShouldBe("wow");
        }
        
        [Fact]
        public void no_arg_return_with_three_setters_case_explicit_setter()
        {
            var result = CodegenScenario.ForBuilds<NoArgGuy, int, double, string>(m =>
            {
                m.Frames.CallConstructor(() => new NoArgGuy(), @call =>
                {
                    @call.Mode = ConstructorCallMode.ReturnValue;
                    @call.Set(x => x.Number);
                    @call.Set(x => x.Double);
                    @call.Set(x => x.String, new Value("Explicit"));
                });

            });

            var noArgGuy = result.Object.Create(11, 1.22, "wow");
            noArgGuy.Number.ShouldBe(11);
            noArgGuy.Double.ShouldBe(1.22);
            noArgGuy.String.ShouldBe("Explicit");
        }

        public class MultiArgGuy
        {
            public int Number { get; }
            public double Amount { get; }
            public string Name { get; }

            public MultiArgGuy(int number)
            {
                Number = number;
            }

            public MultiArgGuy(int number, double amount)
            {
                Number = number;
                Amount = amount;
            }

            public MultiArgGuy(int number, double amount, string name)
            {
                Number = number;
                Amount = amount;
                Name = name;
            }
        }

        [Fact]
        public void one_argument_constructor()
        {
            var result = CodegenScenario.ForBuilds<MultiArgGuy, int>(m =>
            {
                m.Frames.CallConstructor<MultiArgGuy>(() => new MultiArgGuy(0));
                m.Return();
            });

            var guy = result.Object.Create(14);
            guy.Number.ShouldBe(14);
        }

        [Fact] 
        public void two_argument_constructor()
        {
            var result = CodegenScenario.ForBuilds<MultiArgGuy, int, double>(m =>
            {
                m.Frames.CallConstructor<MultiArgGuy>(() => new MultiArgGuy(0, 0));
                m.Return();
            });

            var guy = result.Object.Create(14, 1.23);
            guy.Number.ShouldBe(14);
            guy.Amount.ShouldBe(1.23);
            
        }
        
        [Fact] 
        public void three_argument_constructor()
        {
            var result = CodegenScenario.ForBuilds<MultiArgGuy, int, double, string>(m =>
            {
                m.Frames.CallConstructor<MultiArgGuy>(() => new MultiArgGuy(0, 0, ""));
                m.Return();
            });

            var guy = result.Object.Create(14, 1.23, "Beck");
            guy.Number.ShouldBe(14);
            guy.Amount.ShouldBe(1.23);
            guy.Name.ShouldBe("Beck");
            
        }
        
        [Fact] 
        public void override_an_argument()
        {
            var result = CodegenScenario.ForBuilds<MultiArgGuy, int, double, string>(m =>
            {
                m.Frames.CallConstructor<MultiArgGuy>(() => new MultiArgGuy(0, 0, ""), ctor =>
                {
                    ctor.Parameters[2] = new Value("Kent"); 
                });
                
                m.Return();
            });

            var guy = result.Object.Create(14, 1.23, "Beck");
            guy.Number.ShouldBe(14);
            guy.Amount.ShouldBe(1.23);
            guy.Name.ShouldBe("Kent");
            
        }
        
        [Fact]
        public void activator_with_no_return()
        {
            var result = CodegenScenario.ForAction<NoArgGuyCatcher>(m =>
            {
                m.Frames.CallConstructor(() => new NoArgGuy(), ctor =>
                {
                    ctor.ActivatorFrames.Call<NoArgGuyCatcher>(x => x.Catch(null));
                });
            });
            

            var catcher = new NoArgGuyCatcher();
            result.Object.DoStuff(catcher);
            
            catcher.Guy.ShouldNotBeNull();
        }
        
        [Fact]
        public void activator_with_return()
        {
            var result = CodegenScenario.ForBuilds<NoArgGuy, NoArgGuyCatcher>(m =>
            {
                m.Frames.CallConstructor(() => new NoArgGuy(), ctor =>
                {
                    ctor.Mode = ConstructorCallMode.ReturnValue;
                    ctor.ActivatorFrames.Call<NoArgGuyCatcher>(x => x.Catch(null));
                });

            });
            

            var catcher = new NoArgGuyCatcher();
            var guy = result.Object.Create(catcher);
            
            catcher.Guy.ShouldBeSameAs(guy);
        }
        
        [Fact]
        public void activator_with_nested_in_using()
        {
            var result = CodegenScenario.ForAction<NoArgGuyCatcher>(m =>
            {
                m.Frames.CallConstructor(() => new NoArgGuy(), ctor =>
                {
                    ctor.Mode = ConstructorCallMode.UsingNestedVariable;
                    ctor.ActivatorFrames.Call<NoArgGuyCatcher>(x => x.Catch(null));
                });

            });
            

            var catcher = new NoArgGuyCatcher();
            result.Object.DoStuff(catcher);
            
            catcher.Guy.ShouldNotBeNull();
            catcher.Guy.WasDisposed.ShouldBeTrue();
        }

    }
}