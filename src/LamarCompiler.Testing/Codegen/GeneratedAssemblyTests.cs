using LamarCodeGeneration;
using Shouldly;
using Xunit;

namespace LamarCompiler.Testing.Codegen
{
    public class GeneratedAssemblyTests
    {
        [Fact]
        public void includes_explicit_namespaces()
        {
            var assembly = new GeneratedAssembly(new GenerationRules("LamarCompiler.Generated"));
            assembly.Namespaces.Add(GetType().Namespace);
            
            assembly.AllReferencedNamespaces().ShouldContain(GetType().Namespace);
        }
    }
}