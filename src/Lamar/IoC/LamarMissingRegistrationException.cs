using System;
using Lamar.Codegen;

namespace Lamar.IoC
{
    public class LamarMissingRegistrationException : LamarException
    {
        public LamarMissingRegistrationException(Type serviceType, string name) : base($"Unknown service registration '{name}' of {serviceType.FullNameInCode()}")
        {
        }

        public LamarMissingRegistrationException(Type serviceType) : base($"No service registrations exist or can be derived for {serviceType.FullNameInCode()}")
        {
        }
    }
}