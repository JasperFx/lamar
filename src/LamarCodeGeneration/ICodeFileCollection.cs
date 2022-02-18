using System.Collections.Generic;

namespace LamarCodeGeneration
{
    public interface ICodeFileCollection
    {
        IReadOnlyList<ICodeFile> BuildFiles();
        
        /// <summary>
        /// Appending 
        /// </summary>
        string ChildNamespace { get; }    
    }
}