using System;
using System.IO;
using LamarCodeGeneration.Util;

namespace LamarCodeGeneration
{
    public static class CodeGenerationExtensions
    {
        public static GeneratedAssembly StartAssembly(this IGeneratesCode generator, GenerationRules rules)
        {
            if (generator.ChildNamespace.IsEmpty())
            {
                throw new InvalidOperationException($"Missing {nameof(IGeneratesCode.ChildNamespace)} for {generator}");
            }
                
            var @namespace = $"{rules.ApplicationNamespace}.{generator.ChildNamespace}";
            
            return new GeneratedAssembly(rules, @namespace);
        }

        public static string ToNamespace(this IGeneratesCode generatesCode, GenerationRules rules)
        {
            return $"{rules.ApplicationNamespace}.{generatesCode.ChildNamespace}";
        }

        public static string ToExportDirectory(this IGeneratesCode generator, string exportDirectory)
        {
            if (generator.ChildNamespace.IsEmpty())
            {
                throw new InvalidOperationException($"Missing {nameof(IGeneratesCode.ChildNamespace)} for {generator}");
            }

            var generatorDirectory = exportDirectory;
            var parts = generator.ChildNamespace.Split('.');
            foreach (var part in parts)
            {
                generatorDirectory = Path.Combine(generatorDirectory, part);
            }
            
            new FileSystem().CreateDirectory(generatorDirectory);

            return generatorDirectory;
        }

        public static GeneratedAssembly AssembleTypes(this IGeneratesCode generator, GenerationRules rules)
        {
            var generatedAssembly = generator.StartAssembly(rules);
            foreach (var file in generator.BuildFiles())
            {
                file.AssembleTypes(generatedAssembly);
            }

            return generatedAssembly;
        }
        
    }
}