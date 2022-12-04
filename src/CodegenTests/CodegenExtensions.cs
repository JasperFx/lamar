using JasperFx.CodeGeneration;
using JasperFx.RuntimeCompiler;

namespace CodegenTests;

public static class CodegenExtensions
{
    public static void CompileAll(this GeneratedAssembly assembly)
    {
        new AssemblyGenerator().Compile(assembly);
    }
}