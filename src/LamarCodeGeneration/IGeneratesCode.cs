using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using LamarCodeGeneration.Model;

namespace LamarCodeGeneration
{
    public interface IGeneratesCode
    {
        IReadOnlyList<ICodeFile> BuildFiles();
        
        /// <summary>
        /// Appending 
        /// </summary>
        string ChildNamespace { get; }    
    }

    public interface ICodeFile
    {
        string FileName { get; }

        void AssembleTypes(GeneratedAssembly assembly);
        
        Task<bool> AttachTypes(GenerationRules rules, Assembly assembly, IServiceProvider services);
    }

}