using System;
using LamarCodeGeneration.Util;

namespace LamarCodeGeneration
{
    public class GeneratorCompilationFailureException : Exception
    {
        public GeneratorCompilationFailureException(IGeneratesCode generator, Exception innerException) : base($"Failure when trying to generate code for {generator.GetType().GetFullName()}", innerException)
        {
        }
    }
}