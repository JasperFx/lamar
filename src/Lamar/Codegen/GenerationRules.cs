using System.Collections.Generic;
using System.Reflection;
using Lamar.Codegen.Variables;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.Codegen
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
