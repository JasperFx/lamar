using System;

namespace Lamar
{
    
    /// <summary>
    ///     Allows Lamar to fill in missing registrations by unknown plugin types
    ///     at runtime
    /// </summary>
    [LamarIgnore]
    // SAMPLE: IFamilyPolicy
    public interface IFamilyPolicy : ILamarPolicy
    {
        /// <summary>
        ///     Allows you to create missing registrations for an unknown plugin type
        ///     at runtime.
        ///     Return null if this policy does not apply to the given type
        /// </summary>
        ServiceFamily Build(Type type, ServiceGraph serviceGraph);
    }
    // ENDSAMPLE


}