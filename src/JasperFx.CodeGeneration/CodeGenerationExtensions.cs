using System;
using System.IO;
using JasperFx.CodeGeneration.Model;
using JasperFx.CodeGeneration.Util;
using JasperFx.Core;

namespace JasperFx.CodeGeneration;

public static class CodeGenerationExtensions
{
    public static GeneratedAssembly StartAssembly(this ICodeFileCollection generator, GenerationRules rules)
    {
        if (generator.ChildNamespace.IsEmpty())
        {
            throw new InvalidOperationException(
                $"Missing {nameof(ICodeFileCollection.ChildNamespace)} for {generator}");
        }

        var @namespace = $"{rules.GeneratedNamespace}.{generator.ChildNamespace}";

        return new GeneratedAssembly(rules, @namespace);
    }

    public static string ToNamespace(this ICodeFileCollection codeFileCollection, GenerationRules rules)
    {
        return $"{rules.GeneratedNamespace}.{codeFileCollection.ChildNamespace}";
    }

    public static string ToExportDirectory(this ICodeFileCollection generator, string exportDirectory)
    {
        if (generator.ChildNamespace.IsEmpty())
        {
            throw new InvalidOperationException(
                $"Missing {nameof(ICodeFileCollection.ChildNamespace)} for {generator}");
        }

        var generatorDirectory = exportDirectory;
        var parts = generator.ChildNamespace.Split('.');
        foreach (var part in parts) generatorDirectory = Path.Combine(generatorDirectory, part);

        new FileSystem().CreateDirectory(generatorDirectory);

        return generatorDirectory;
    }

    public static GeneratedAssembly AssembleTypes(this ICodeFileCollection generator, GenerationRules rules)
    {
        var generatedAssembly = generator.StartAssembly(rules);
        foreach (var file in generator.BuildFiles()) file.AssembleTypes(generatedAssembly);

        return generatedAssembly;
    }

    /// <summary>
    ///     Add a new string constant to the generated type
    /// </summary>
    /// <param name="generatedType"></param>
    /// <param name="constantName"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Setter AddStringConstant(this GeneratedType generatedType, string constantName, string value)
    {
        var setter = Setter.Constant(constantName, Constant.ForString(value));
        generatedType.Setters.Add(setter);

        return setter;
    }

    /// <summary>
    ///     Creates a new string constant value on the holding type and uses that value as the return
    ///     value for this method
    /// </summary>
    /// <param name="frames"></param>
    /// <param name="constantName"></param>
    /// <param name="value"></param>
    public static void ReturnNewStringConstant(this FramesCollection frames,
        string constantName, string value)
    {
        var setter = frames.ParentMethod.ParentType.AddStringConstant(constantName, value);
        frames.Return(setter);
    }
}