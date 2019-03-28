using System.Reflection;
using Lamar.IoC.Frames;
using LamarCodeGeneration.Model;

namespace Lamar.IoC.Instances
{
    public class CtorArg
    {
        public ParameterInfo Parameter { get; }
        public Instance Instance { get; }

        public CtorArg(ParameterInfo parameter, Instance instance)
        {
            Parameter = parameter;
            Instance = instance;
            
            if (instance.IsInlineDependency() || instance is LambdaInstance && instance.ServiceType.IsGenericType)
            {
                instance.Name = Parameter.Name;
            }
        }

        public Variable Resolve(ResolverVariables variables, BuildMode mode)
        {
            if (Instance.IsInlineDependency())
            {
                var variable = Instance.CreateInlineVariable(variables);

                
                // HOKEY. Might need some smarter way of doing this. Helps to disambiguate
                // between ctor args of nested decorators
                if (!(variable is Setter))
                {
                    variable.OverrideName(variable.Usage + "_inline_" + ++variables.VariableSequence);
                }
                

                return variable;
            }
                
            var inner = variables.Resolve(Instance, mode);
            
            return Parameter.IsOptional 
                ? new OptionalArgumentVariable(inner, Parameter) 
                : inner;
        }
    }
}