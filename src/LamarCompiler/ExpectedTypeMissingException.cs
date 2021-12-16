using System;

namespace LamarCompiler
{
    /// <summary>
    /// Denotes an expected set of pre-built, generated types
    /// that were missing in the application assembly
    /// </summary>
    public class ExpectedTypeMissingException : Exception
    {
        public ExpectedTypeMissingException(string message) : base(message)
        {
        }
    }
}