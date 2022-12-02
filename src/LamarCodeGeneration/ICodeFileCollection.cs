using System.Collections.Generic;

namespace LamarCodeGeneration;

public interface ICodeFileCollection
{
    /// <summary>
    ///     Appending
    /// </summary>
    string ChildNamespace { get; }

    GenerationRules Rules { get; }
    IReadOnlyList<ICodeFile> BuildFiles();
}