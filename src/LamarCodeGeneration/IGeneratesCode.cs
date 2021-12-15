using System;
using System.Reflection;
using System.Threading.Tasks;
using LamarCodeGeneration.Model;

namespace LamarCodeGeneration
{
    public interface IGeneratesCode
    {
        IServiceVariableSource AssemblyTypes(GenerationRules rules, GeneratedAssembly assembly);
        
        // TODO -- fold these together
        Task AttachPreBuiltTypes(GenerationRules rules, Assembly assembly, IServiceProvider services);

        Task AttachGeneratedTypes(GenerationRules rules, IServiceProvider services);
        
        /// <summary>
        /// This is strictly for diagnostics to identify the code generating strategy
        /// </summary>
        string CodeType { get; }
    }
}