using BaselineTypeDiscovery;

namespace Lamar.Scanning.Conventions
{
    // SAMPLE: IRegistrationConvention
    /// <summary>
    ///     Used to create custom type scanning conventions
    /// </summary>
    public interface IRegistrationConvention
    {
        void ScanTypes(TypeSet types, ServiceRegistry services);
    }

    // ENDSAMPLE
}