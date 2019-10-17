using System;
using System.Reflection;
using LamarCodeGeneration.Model;

namespace LamarCodeGeneration
{
    public interface IGeneratesCode
    {
        IServiceVariableSource AssemblyTypes(GenerationRules rules, GeneratedAssembly assembly);
        void AttachPreBuiltTypes(Assembly assembly, IServiceProvider services);

        void AttachGeneratedTypes(IServiceProvider services);
        
        /// <summary>
        /// This is strictly for diagnostics to identify the code generating strategy
        /// </summary>
        string CodeType { get; }
    }
}