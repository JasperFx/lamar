using System.Linq;
using LamarCompiler.Frames;
using LamarCompiler.Model;
using Shouldly;
using Xunit;

namespace LamarCompiler.Testing.Codegen
{
    public class when_building_a_method_call_for_a_tuple
    {
        private readonly MethodCall theCall= MethodCall.For<MethodTarget>(x => x.ReturnTuple());

        
        [Fact]
        public void override_variable_name_of_one_of_the_inners()
        {
            theCall.Creates.ElementAt(0).OverrideName("mauve");
            theCall.ReturnVariable.Usage.ShouldBe("(var mauve, var blue, var green)");
        }
        

        [Fact]
        public void return_variable_usage()
        {
            theCall.ReturnVariable.Usage.ShouldBe("(var red, var blue, var green)");
        }
        
        [Fact]
        public void creates_does_not_contain_the_return_variable()
        {
            theCall.Creates.ShouldNotContain(theCall.ReturnVariable);
        }
        
        [Fact]
        public void has_creation_variables_for_the_tuple_types()
        {
            theCall.Creates.ShouldHaveTheSameElementsAs(Variable.For<Red>(), Variable.For<Blue>(), Variable.For<Green>());
        }
    }
}