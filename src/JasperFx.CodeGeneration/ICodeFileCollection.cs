using System.Collections.Generic;

namespace JasperFx.CodeGeneration;

public interface ICodeFileCollection
{
    /// <summary>
    ///     Appending
    /// </summary>
    string ChildNamespace { get; }

    GenerationRules Rules { get; }
    IReadOnlyList<ICodeFile> BuildFiles();
}