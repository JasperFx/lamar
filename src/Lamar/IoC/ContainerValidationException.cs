using System;

namespace Lamar.IoC
{
    public class ContainerValidationException : Exception
    {
        public ContainerValidationException(string message) : base(message)
        {
        }
    }
}