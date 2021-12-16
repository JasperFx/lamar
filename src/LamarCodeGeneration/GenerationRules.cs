using System.Collections.Generic;
using System.Reflection;
using LamarCodeGeneration.Model;

namespace LamarCodeGeneration
{

    public enum TypeLoadMode
    {
        /// <summary>
        /// Always generate new types at runtime. This is appropriate for
        /// development time when configuration may be in flux
        /// </summary>
        Dynamic,
        
        /// <summary>
        /// Try to load generated types from the target application assembly,
        /// but generate types if there is no pre-built types and export
        /// the new source code
        /// </summary>
        Auto,
        
        /// <summary>
        /// Types must be loaded from the pre-built application assembly, or
        /// the loading will throw exceptions
        /// </summary>
        Static
    }
    
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

        public TypeLoadMode TypeLoadMode { get; set; } = TypeLoadMode.Dynamic;
        
        public string GeneratedCodeOutputPath {get;set;} = "Internal/Generated";

        public readonly IList<IVariableSource> Sources = new List<IVariableSource>();

        public readonly IList<Assembly> Assemblies = new List<Assembly>();
        
        public readonly IDictionary<string, object> Properties = new Dictionary<string, object>();

        public Assembly ApplicationAssembly { get; set; } = Assembly.GetEntryAssembly();
        
    }


}
