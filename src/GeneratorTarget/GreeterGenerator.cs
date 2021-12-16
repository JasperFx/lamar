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

    public class GreeterFile : ICodeFile
    {
        private GeneratedType _type;

        public GreeterFile(string name)
        {
            FileName = name;
            TypeName = name;
        }

        public string TypeName { get; set; }

        public string FileName { get; }
        public void AssembleTypes(GeneratedAssembly assembly)
        {
            _type = assembly.AddType(TypeName, typeof(IGreeter));
            var method = _type.MethodFor(nameof(IGreeter.Greetings));
            method.Frames.Code($"return \"{FileName}\";");
        }

        public Task<bool> AttachTypes(GenerationRules rules, Assembly assembly, IServiceProvider services)
        {
            var type = _type.FindType(assembly.GetExportedTypes());
            if (type == null)
            {
                throw new Exception("Cannot find type with full name " + _type.FullName);
            }

            return Task.FromResult(true);
        }
    }
    
    public class GreeterGenerator : IGeneratesCode
    {
        public IReadOnlyList<ICodeFile> BuildFiles()
        {
            return new List<ICodeFile>
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
        public IReadOnlyList<ICodeFile> BuildFiles()
        {
            return new List<ICodeFile>
            {
                new GreeterFile("Laters"),
            };
        }

        public string ChildNamespace { get; } = "Helpers.Greeters2";
    }
}