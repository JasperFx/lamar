using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Lamar.IoC.Instances;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;

namespace GeneratorTarget
{
    public interface IGreeter
    {
        string Greetings();
    }

    public class GreeterFile : CodeFile
    {
        private GeneratedType _type;

        public GreeterFile(string name)
        {
            FileName = name;
            TypeName = name;
        }

        public string TypeName { get; set; }

        public override string FileName { get; }
        public override void AssembleTypes(GeneratedAssembly assembly)
        {
            _type = assembly.AddType(TypeName, typeof(IGreeter));
            var method = _type.MethodFor(nameof(IGreeter.Greetings));
            method.Frames.Code($"return \"{FileName}\";");
        }

        public override Task AttachTypes(GenerationRules rules, Assembly assembly, IServiceProvider services)
        {
            var type = _type.FindType(assembly.GetExportedTypes());
            if (type == null)
            {
                throw new Exception("Cannot find type with full name " + _type.FullName);
            }

            return Task.CompletedTask;
        }
    }
    
    public class GreeterGenerator : IGeneratesCode
    {
        public IReadOnlyList<CodeFile> BuildFiles()
        {
            return new List<CodeFile>
            {
                new GreeterFile("Hey"),
                new GreeterFile("Bye"),
                new GreeterFile("Hola"),
                new GreeterFile("Ciao"),
            };
        }

        public string ChildNamespace { get; } = "Helpers.Greeters";
    }
    
    public class GreeterGenerator2 : IGeneratesCode
    {
        public IReadOnlyList<CodeFile> BuildFiles()
        {
            return new List<CodeFile>
            {
                new GreeterFile("Laters"),
            };
        }

        public string ChildNamespace { get; } = "Helpers.Greeters2";
    }
}