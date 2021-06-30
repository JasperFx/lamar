namespace Lamar
{
    /// <summary>
    /// Used to apply "this must win" service overrides regardless of the order of
    /// registration
    /// </summary>
    internal class LamarOverrides : IRegistrationPolicy
    {
        public void Apply(ServiceRegistry services)
        {
            services.AddRange(Overrides);
        }

        public ServiceRegistry Overrides { get; } = new ServiceRegistry();
    }
}