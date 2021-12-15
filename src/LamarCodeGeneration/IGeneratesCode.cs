using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using LamarCodeGeneration.Model;

namespace LamarCodeGeneration
{
    public interface IGeneratesCode
    {
        IReadOnlyList<CodeFile> BuildFiles();
        
        /// <summary>
        /// Appending 
        /// </summary>
        string ChildNamespace { get; }    
    }

    public abstract class CodeFile
    {
        public abstract string FileName { get; }

        public abstract void AssembleTypes(GeneratedAssembly assembly);
        
        public abstract Task AttachTypes(GenerationRules rules, Assembly assembly, IServiceProvider services);
    }

}