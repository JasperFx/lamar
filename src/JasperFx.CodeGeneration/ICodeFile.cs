using System;
using System.Reflection;
using System.Threading.Tasks;

namespace JasperFx.CodeGeneration;

public interface ICodeFile
{
    string FileName { get; }

    void AssembleTypes(GeneratedAssembly assembly);

    Task<bool> AttachTypes(GenerationRules rules, Assembly assembly, IServiceProvider services,
        string containingNamespace);

    bool AttachTypesSynchronously(GenerationRules rules, Assembly assembly, IServiceProvider services,
        string containingNamespace);
}