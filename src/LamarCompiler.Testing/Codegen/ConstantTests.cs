using System;
using LamarCodeGeneration.Model;
using Shouldly;
using Xunit;

namespace LamarCompiler.Testing.Codegen
{
    public class ConstantTests
    {
        [Fact]
        public void get_constant_for_type()
        {
            var variable = Constant.ForType(GetType());
            variable.VariableType.ShouldBe(typeof(Type));
            variable.Usage.ShouldBe("typeof(LamarCompiler.Testing.Codegen.ConstantTests)");
        }
    }
}