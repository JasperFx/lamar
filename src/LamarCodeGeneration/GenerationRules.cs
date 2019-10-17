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
        
        public GenerationRules()
        {
        }

        public string ApplicationNamespace { get; set; } = "JasperGenerated";
        
        public string GeneratedCodeOutputPath {get;set;} = "Internal/Generated";

        public readonly IList<IVariableSource> Sources = new List<IVariableSource>();

        public readonly IList<Assembly> Assemblies = new List<Assembly>();
        
        public readonly IDictionary<string, object> Properties = new Dictionary<string, object>();
    }


}
