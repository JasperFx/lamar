namespace Lamar.Scanning.Conventions;

/// <summary>
///     Controls the behavior of the type scanning registrations if there are prior, matching
///     registrations
/// </summary>
public enum OverwriteBehavior
{
    /// <summary>
    ///     Add registrations for the service type even if there are other registrations
    ///     for this service type
    /// </summary>
    Always,

    /// <summary>
    ///     Add registrations if the service type/implementation type is new
    /// </summary>
    NewType,

    /// <summary>
    ///     Do not add any registrations for the service type if there are any other registrations
    /// </summary>
    Never
}