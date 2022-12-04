namespace JasperFx.CodeGeneration.Model;

public enum VariableCreation
{
    /// <summary>
    ///     Means that this variable has to be directly created within
    ///     the code frame chain
    /// </summary>
    Created,

    /// <summary>
    ///     Variable is injected into the handler class itself
    /// </summary>
    Injected,

    /// <summary>
    ///     Variable is built by a frame
    /// </summary>
    BuiltByFrame
}