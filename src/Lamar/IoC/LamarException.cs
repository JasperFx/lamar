using System;

namespace Lamar.IoC
{
    public class LamarException : Exception
    {
        public LamarException(string message) : base(message)
        {
        }

        public LamarException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}