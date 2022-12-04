using System;
using JasperFx.CodeGeneration.Model;
using Shouldly;
using Xunit;

namespace CodegenTests.Codegen;

public class ConstantTests
{
    [Fact]
    public void get_constant_for_type()
    {
        var variable = Constant.ForType(GetType());
        variable.VariableType.ShouldBe(typeof(Type));
        variable.Usage.ShouldBe("typeof(CodegenTests.Codegen.ConstantTests)");
    }
}