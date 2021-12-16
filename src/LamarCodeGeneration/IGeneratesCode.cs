using System.Collections.Generic;

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
}