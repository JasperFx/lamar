using LamarCompiler;

namespace Lamar.Testing
{
    public static class CodegenExtensions
    {
        public static void CompileAll(this GeneratedAssembly assembly)
        {
            new AssemblyGenerator().Compile(assembly);
        }
    }
}