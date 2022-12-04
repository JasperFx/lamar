using System;

namespace JasperFx.CodeGeneration;

public class GeneratorCompilationFailureException : Exception
{
    public GeneratorCompilationFailureException(ICodeFileCollection generator, Exception innerException) : base(
        $"Failure when trying to generate code for {generator.GetType().FullNameInCode()}", innerException)
    {
    }
}