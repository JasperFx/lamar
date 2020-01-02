using System.Linq;
using System.Threading.Tasks;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using NSubstitute;
using Shouldly;
using Xunit;
using Xunit.Sdk;

namespace LamarCompiler.Testing.Codegen
{
    public class MethodCallTester
    {
        [Fact]
        public void determine_return_value_of_simple_type()
        {
            var @call = MethodCall.For<MethodCallTarget>(x => x.GetValue());
            @call.ReturnVariable.ShouldNotBeNull();

            @call.ReturnVariable.VariableType.ShouldBe(typeof(string));
            @call.ReturnVariable.Usage.ShouldBe("result_of_GetValue");
            @call.ReturnVariable.Creator.ShouldBeSameAs(@call);
        }

        [Fact]
        public void determine_return_value_of_not_simple_type()
        {
            var @call = MethodCall.For<MethodCallTarget>(x => x.GetError());
            @call.ReturnVariable.ShouldNotBeNull();

            @call.ReturnVariable.VariableType.ShouldBe(typeof(ErrorMessage));
            @call.ReturnVariable.Usage.ShouldBe(Variable.DefaultArgName(typeof(ErrorMessage)));
            @call.ReturnVariable.Creator.ShouldBeSameAs(@call);
        }

        [Fact]
        public void no_return_variable_on_void_method()
        {
            var @call = MethodCall.For<MethodCallTarget>(x => x.Go(null));
            @call.ReturnVariable.ShouldBeNull();
        }

        [Fact]
        public void determine_return_value_of_Task_of_T_simple()
        {
            var @call = MethodCall.For<MethodCallTarget>(x => x.GetString());
            @call.ReturnVariable.ShouldNotBeNull();

            @call.ReturnVariable.VariableType.ShouldBe(typeof(string));
            @call.ReturnVariable.Usage.ShouldBe("result_of_GetString");
            @call.ReturnVariable.Creator.ShouldBeSameAs(@call);
        }


        [Fact]
        public void determine_return_value_of_not_simple_type_in_a_task()
        {
            var @call = MethodCall.For<MethodCallTarget>(x => x.GetAsyncError());
            @call.ReturnVariable.ShouldNotBeNull();

            @call.ReturnVariable.VariableType.ShouldBe(typeof(ErrorMessage));
            @call.ReturnVariable.Usage.ShouldBe(Variable.DefaultArgName(typeof(ErrorMessage)));
            @call.ReturnVariable.Creator.ShouldBeSameAs(@call);
        }

        [Fact]
        public void explicitly_set_parameter_by_variable_type()
        {
            var @call = MethodCall.For<MethodCallTarget>(x => x.DoSomething(0, 0, null));

            var stringVariable = Variable.For<string>();
            var generalInt = Variable.For<int>();

            // Only one of that type, so it works
            @call.TrySetArgument(stringVariable)
                .ShouldBeTrue();

            @call.Arguments[2].ShouldBeSameAs(stringVariable);

            // Multiple parameters of the same type, nothing
            @call.TrySetArgument(generalInt).ShouldBeFalse();
            @call.Arguments[0].ShouldBeNull();
            @call.Arguments[1].ShouldBeNull();
        }

        [Fact]
        public void explicitly_set_parameter_by_variable_type_and_name()
        {
            var @call = MethodCall.For<MethodCallTarget>(x => x.DoSomething(0, 0, null));

            var generalInt = Variable.For<int>();

            @call.TrySetArgument("count", generalInt)
                .ShouldBeTrue();

            @call.Arguments[0].ShouldBeNull();
            @call.Arguments[1].ShouldBeSameAs(generalInt);
            @call.Arguments[2].ShouldBeNull();
        }

        [Fact]
        public void find_handler_if_not_local()
        {
            var @call = MethodCall.For<MethodCallTarget>(x => x.GetValue());
            var chain = Substitute.For<IMethodVariables>();

            var handler = Variable.For<MethodCallTarget>();
            chain.FindVariable(typeof(MethodCallTarget)).Returns(handler);
            
            @call.FindVariables(chain).Single()
                .ShouldBeSameAs(handler);
        }
        
        [Fact]
        public void find_no_handler_if_local()
        {
            var @call = MethodCall.For<MethodCallTarget>(x => x.GetValue());
            @call.IsLocal = true;
            
            var chain = Substitute.For<IMethodVariables>();

            var handler = Variable.For<MethodCallTarget>();
            chain.FindVariable(typeof(MethodCallTarget)).Returns(handler);
            
            @call.FindVariables(chain).Any().ShouldBeFalse();
        }

        [Fact]
        public void find_no_handler_variable_if_it_is_static()
        {
            var @call = new MethodCall(typeof(MethodCallTarget), nameof(MethodCallTarget.GoStatic));
            @call.IsLocal = true;
            
            var chain = Substitute.For<IMethodVariables>();

            var handler = Variable.For<MethodCallTarget>();
            chain.FindVariable(typeof(MethodCallTarget)).Returns(handler);
            
            @call.FindVariables(chain).Any().ShouldBeFalse();
        }

        [Fact]
        public void find_simple_variable_by_name_and_type()
        {
            var age = Variable.For<int>("age");
            var count = Variable.For<int>("count");

            var name = Variable.For<string>();
            
            var variables = new StubMethodVariables();
            variables.Store(age);
            variables.Store(count);
            variables.Store(name);
            
            var @call = MethodCall.For<MethodCallTarget>(x => x.DoSomething(0, 0, null));
            @call.IsLocal = true;

            var found = @call.FindVariables(variables).ToArray();
            
            @call.Arguments[0].ShouldBe(age);
            @call.Arguments[1].ShouldBe(count);
            @call.Arguments[2].ShouldBe(name);
        }

        [Fact]
        public void find_variables_returns_all_the_set_arguments_too()
        {
            var age = Variable.For<int>("age");
            var count = Variable.For<int>("count");

            var name = Variable.For<string>();
            
            var variables = new StubMethodVariables();
            //variables.Store(age);
            variables.Store(count);
            variables.Store(name);
            
            var @call = MethodCall.For<MethodCallTarget>(x => x.DoSomething(0, 0, null));
            @call.IsLocal = true;
            @call.TrySetArgument("age", age).ShouldBeTrue();

            var found = @call.FindVariables(variables).ToArray();
            
            found.ShouldContain(age);
            found.ShouldContain(count);
            found.ShouldContain(name);
        }

        [Fact]
        public void use_a_type_alias()
        {
            var variables = new StubMethodVariables();
            var basketball = Variable.For<Basketball>();
            variables.Store(basketball);

            var @call = MethodCall.For<MethodCallTarget>(x => x.Throw(null));
            @call.IsLocal = true;

            @call.Aliases[typeof(Ball)] = typeof(Basketball);

            @call.FindVariables(variables)
                .Single()
                .ShouldBeOfType<CastVariable>()
                .Inner
                .ShouldBeSameAs(basketball);
        }
        
        [Fact]
        public void default_disposal_mode_is_using()
        {
            MethodCall.For<MethodCallTarget>(x => x.Throw(null))
                .DisposalMode.ShouldBe(DisposalMode.UsingBlock);
        }
        
        [Fact]
        public void use_with_output_arguments_and_no_return_value()
        {
            var @call = new MethodCall(typeof(MethodCallTarget), nameof(MethodCallTarget.WithOuts));
            
            @call.ReturnVariable.ShouldBeNull();

            @call.Arguments[0].ShouldBeNull();
            @call.Arguments[1].ShouldBeOfType<OutArgument>();
            @call.Arguments[2].ShouldBeOfType<OutArgument>();
            
            @call.Creates.Select(x => x.VariableType)
                .ShouldHaveTheSameElementsAs(typeof(string), typeof(int));
        }
        
        
        [Fact]
        public void use_with_output_arguments_and_return_value()
        {
            var @call = new MethodCall(typeof(MethodCallTarget), nameof(MethodCallTarget.ReturnAndOuts));
            
            @call.ReturnVariable.VariableType.ShouldBe(typeof(bool));
            
            @call.Creates.Select(x => x.VariableType)
                .ShouldHaveTheSameElementsAs(typeof(bool),typeof(string), typeof(int));
        }
        
        [Fact]
        public void generate_code_with_output_variables()
        {
            var @call = new MethodCall(typeof(MethodCallTarget), nameof(MethodCallTarget.ReturnAndOuts));
            @call.Arguments[0] = new Variable(typeof(string), "input");
            @call.IsLocal = true;

            var writer = new SourceWriter();
            @call.GenerateCode(new GeneratedMethod("Go", typeof(void)), writer);
            
            writer.Code().Trim().ShouldBe("var result_of_ReturnAndOuts = ReturnAndOuts(input, out var string, out var int32);");
        }

        [Fact]
        public void write_comment_text_if_exists()
        {
            var @call = new MethodCall(typeof(MethodCallTarget), nameof(MethodCallTarget.ReturnAndOuts));
            @call.Arguments[0] = new Variable(typeof(string), "input");
            @call.IsLocal = true;

            @call.CommentText = "Hey.";
            
            var writer = new SourceWriter();
            @call.GenerateCode(new GeneratedMethod("Go", typeof(void)), writer);
            
            writer.Code().Trim().ShouldStartWith("// Hey.");
        }
        

    }

    public class Ball
    {
        
    }

    public class Basketball : Ball
    {
        
    }

    public class MethodCallTarget
    {
        public void Throw(Ball ball)
        {
            
        }

        public void WithOuts(string in1, out string out1, out int out2)
        {
            out1 = "foo";
            out2 = 2;
        }

        public bool ReturnAndOuts(string in1, out string out1, out int out2)
        {
            out1 = "foo";
            out2 = 2;

            return true;
        }
        
        public string GetValue()
        {
            return "foo";
        }

        public static void GoStatic()
        {
            
        }

        public ErrorMessage GetError()
        {
            return null;
        }

        public Task<ErrorMessage> GetAsyncError()
        {
            return null;
        }

        public void Go(string text)
        {

        }

        public void DoSomething(int age, int count, string name)
        {

        }

        public Task<string> GetString()
        {
            return Task.FromResult("foo");
        }
    }
}
