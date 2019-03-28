using System.Reflection;
using LamarCodeGeneration.Model;

namespace Lamar.IoC.Instances
{
    /// <summary>
    /// Used to fulfill named arguments to a constructor function
    /// </summary>
    public class OptionalArgumentVariable : Variable
    {
        private readonly ParameterInfo _parameter;

        public OptionalArgumentVariable(Variable inner, ParameterInfo parameter) : base(inner.VariableType)
        {
            Inner = inner;
            _parameter = parameter;
            
            Dependencies.Add(Inner);
              
        }

        public Variable Inner { get; }

        public override string Usage
        {
            get => $"{_parameter.Name}: {Inner.Usage}";
            protected set
            {
                // nothing
            }
        }


    }
}