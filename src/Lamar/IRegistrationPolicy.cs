namespace Lamar;

/// <summary>
///     Policy that can impact the entire service collection of registrations
///     on container constructions
/// </summary>
public interface IRegistrationPolicy : ILamarPolicy
{
    void Apply(ServiceRegistry services);
}