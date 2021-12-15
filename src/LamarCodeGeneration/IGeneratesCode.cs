using System;
using System.Reflection;
using System.Threading.Tasks;
using LamarCodeGeneration.Model;

namespace LamarCodeGeneration
{
    public interface IGeneratesCode
    {
        IServiceVariableSource AssemblyTypes(GenerationRules rules, GeneratedAssembly assembly);
        
        Task AttachTypes(GenerationRules rules, Assembly assembly, IServiceProvider services);

        /// <summary>
        /// This is strictly for diagnostics to identify the code generating strategy
        /// </summary>
        string CodeType { get; }
    }

}