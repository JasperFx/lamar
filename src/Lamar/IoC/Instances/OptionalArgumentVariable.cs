using System.Reflection;
using Lamar.Codegen.Variables;

namespace Lamar.IoC.Instances
{
    /// <summary>
    /// Used to fulfill named arguments to a constructor function
    /// </summary>
    public class OptionalArgumentVariable : Variable
    {
        private readonly Variable _inner;
        private readonly ParameterInfo _parameter;

        public OptionalArgumentVariable(Variable inner, ParameterInfo parameter) : base(inner.VariableType)
        {
            _inner = inner;
            _parameter = parameter;
            
            Dependencies.Add(_inner);
              
        }

        public override string Usage
        {
            get { return $"{_parameter.Name}: {_inner.Usage}"; }
            protected set
            {
                // nothing
            }
        }
    }
}