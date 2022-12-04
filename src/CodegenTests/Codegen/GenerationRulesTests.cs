using JasperFx.CodeGeneration;
using Shouldly;
using Xunit;

namespace CodegenTests.Codegen;

public class GenerationRulesTests
{
    [Fact]
    public void source_code_writing_is_enabled_by_default()
    {
        new GenerationRules().SourceCodeWritingEnabled.ShouldBeTrue();
    }
}