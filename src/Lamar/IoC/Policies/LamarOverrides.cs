namespace Lamar;

/// <summary>
///     Used to apply "this must win" service overrides regardless of the order of
///     registration
/// </summary>
internal class LamarOverrides : IRegistrationPolicy
{
    public ServiceRegistry Overrides { get; } = new();

    public void Apply(ServiceRegistry services)
    {
        services.AddRange(Overrides);
    }
}