using System.Collections.Generic;
using System.Reflection;
using LamarCodeGeneration.Model;

namespace LamarCodeGeneration
{
    public class GenerationRules
    {
        public GenerationRules(string applicationNamespace)
        {
            ApplicationNamespace = applicationNamespace;
        }

        public string ApplicationNamespace { get; }

        public readonly IList<IVariableSource> Sources = new List<IVariableSource>();

        public readonly IList<Assembly> Assemblies = new List<Assembly>();
    }


}
