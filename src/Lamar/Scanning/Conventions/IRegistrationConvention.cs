using JasperFx.TypeDiscovery;

namespace Lamar.Scanning.Conventions
{
    #region sample_IRegistrationConvention
    /// <summary>
    ///     Used to create custom type scanning conventions
    /// </summary>
    public interface IRegistrationConvention
    {
        void ScanTypes(TypeSet types, ServiceRegistry services);
    }

    #endregion
}