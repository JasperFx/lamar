namespace Lamar.IoC;

public enum BuildMode
{
    /// <summary>
    ///     Build as if it is used within a handler like
    ///     a Jasper message handler or http handler
    /// </summary>
    Inline,

    /// <summary>
    ///     Build as a dependency of another object
    /// </summary>
    Dependency,

    /// <summary>
    ///     Build as a return value inside of a resolver
    /// </summary>
    Build
}