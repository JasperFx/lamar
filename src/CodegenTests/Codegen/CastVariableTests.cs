﻿using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Model;
using Shouldly;
using Xunit;

namespace CodegenTests.Codegen;

public class CastVariableTests
{
    [Fact]
    public void does_the_cast()
    {
        var inner = Variable.For<Basketball>();
        var cast = new CastVariable(inner, typeof(Ball));

        cast.Usage.ShouldBe($"(({typeof(Ball).FullNameInCode()}){inner.Usage})");
    }
}