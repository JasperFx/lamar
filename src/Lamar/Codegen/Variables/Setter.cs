using System;
using System.Linq;
using Lamar.Compilation;

namespace Lamar.Codegen.Variables
{
    public class Setter : Variable
    {
        public Setter(Type variableType) : base(variableType)
        {
        }

        public Setter(Type variableType, string name) : base(variableType, name)
        {
            PropName = name;
        }

        public string PropName { get; set; }

        public void WriteDeclaration(ISourceWriter writer)
        {
            writer.Write($"public {VariableType.FullNameInCode()} {PropName} {{get; set;}}");
        }
        
        /// <summary>
        /// Value to be set upon creating an instance of the class
        /// </summary>
        public object InitialValue { get; set; }

        public void SetInitialValue(object @object)
        {
            var property = @object.GetType().GetProperty(Usage);
            property.SetValue(@object, InitialValue);
        }
    }
}