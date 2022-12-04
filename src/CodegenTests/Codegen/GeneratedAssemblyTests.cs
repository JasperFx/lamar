using System.Linq;
using JasperFx.CodeGeneration;
using JasperFx.Core;
using Shouldly;
using Xunit;

namespace CodegenTests.Codegen;

public class GeneratedAssemblyTests
{
    [Fact]
    public void includes_explicit_namespaces()
    {
        var assembly = new GeneratedAssembly(new GenerationRules("JasperFx.RuntimeCompiler.Generated"));
        assembly.UsingNamespaces.Add(GetType().Namespace);

        assembly.AllReferencedNamespaces().ShouldContain(GetType().Namespace);
    }

    [Fact]
    public void write_header_and_footer()
    {
        var assembly = new GeneratedAssembly(new GenerationRules("JasperFx.RuntimeCompiler.Generated"));
        assembly.Header = new OneLineComment("Start here.");
        assembly.Footer = new OneLineComment("End here.");

        var code = assembly.GenerateCode().ReadLines().ToArray();
        code[0].ShouldBe("// Start here.");
        code.Last().ShouldBe("// End here.");
    }

    [Fact]
    public void write_header_and_footer_2()
    {
        var assembly = new GeneratedAssembly(new GenerationRules("JasperFx.RuntimeCompiler.Generated"));
        assembly.Header = ConditionalCompilation.If("NET");
        assembly.Footer = ConditionalCompilation.EndIf();

        var code = assembly.GenerateCode().ReadLines().ToArray();
        code[0].ShouldBe("#if NET");
        code.Last().ShouldBe("#endif");
    }
}