using LamarCodeGeneration;
using Shouldly;
using Xunit;

namespace LamarCompiler.Testing.Codegen;

public class GenerationRulesTests
{
    [Fact]
    public void source_code_writing_is_enabled_by_default()
    {
        new GenerationRules().SourceCodeWritingEnabled.ShouldBeTrue();
    }
}