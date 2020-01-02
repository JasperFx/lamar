using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using LamarCodeGeneration.Model;
using LamarCodeGeneration.Util;

namespace LamarCodeGeneration.Frames
{
    public enum DisposalMode
    {
        UsingBlock,
        None
    
    }
    
    public class MethodCall : Frame
    {
        public Dictionary<Type, Type> Aliases { get; } = new Dictionary<Type, Type>();

        public Type HandlerType { get; }
        public MethodInfo Method { get; }
        
        
        public Variable ReturnVariable { get; private set; }

        public static MethodCall For<T>(Expression<Action<T>> expression)
        {
            var method = ReflectionHelper.GetMethod(expression);

            return new MethodCall(typeof(T), method);
        }

        public MethodCall(Type handlerType, string methodName) : this(handlerType, handlerType.GetMethod(methodName))
        {
            
        }


        public MethodCall(Type handlerType, MethodInfo method) : base(method.IsAsync())
        {
            HandlerType = handlerType;
            Method = method;

            Type returnType = correctedReturnType(method.ReturnType);
            if (returnType != null)
            {
#if !NET461
                

                if (returnType.IsValueTuple())
                {
                    var values = returnType.GetGenericArguments().Select(x => new Variable(x, this)).ToArray();
                
                    ReturnVariable = new ValueTypeReturnVariable(returnType, values);
                }
                else
                {
#endif
                    var name = returnType.IsSimple() || returnType == typeof(object) || returnType == typeof(object[])
                        ? "result_of_" + method.Name
                        : Variable.DefaultArgName(returnType);
                    

                    ReturnVariable = new Variable(returnType, name, this); 
                    
#if !NET461
                }
#endif
            }
            


            var parameters = method.GetParameters();
            Arguments = new Variable[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                if (param.IsOut)
                {
                    var paramType = param.ParameterType.IsByRef ? param.ParameterType.GetElementType() : param.ParameterType;
                    Arguments[i] = new OutArgument(paramType, this);
                }
            }
            
        }
        
        /// <summary>
        /// Optional text to write as a descriptive comment
        /// in the generated code
        /// </summary>
        public string CommentText { get; set; }

        private Type correctedReturnType(Type type)
        {
            if (type == typeof(Task) || type == typeof(void)) return null;

            if (type.CanBeCastTo<Task>()) return type.GetGenericArguments().First();

            return type;
        }
        

        /// <summary>
        /// Call a method on the current object
        /// </summary>
        public bool IsLocal { get; set; }

        public Variable Target { get; set; }


        private Variable findVariable(ParameterInfo param, IMethodVariables chain)
        {
            var type = param.ParameterType;

            if (Aliases.ContainsKey(type))
            {
                var actualType = Aliases[type];
                var inner = chain.FindVariable(actualType);
                return new CastVariable(inner, type);
            }

            return chain.TryFindVariableByName(type, param.Name, out var variable) ? variable : chain.FindVariable(type);
        }

        public Variable[] Arguments { get; }

        public DisposalMode DisposalMode { get; set; } = DisposalMode.UsingBlock;

        public bool TrySetArgument(Variable variable)
        {
            var parameters = Method.GetParameters().Select(x => x.ParameterType).ToArray();
            if (parameters.Count(x => variable.VariableType.CanBeCastTo(x)) != 1) return false;
            
            var index = Array.IndexOf(parameters, variable.VariableType);
            Arguments[index] = variable;

            return true;
        }

        public bool TrySetArgument(string parameterName, Variable variable)
        {
            var parameters = Method.GetParameters().ToArray();
            var matching = parameters.FirstOrDefault(x =>
                variable.VariableType.CanBeCastTo(x.ParameterType) && x.Name == parameterName);

            if (matching == null) return false;

            var index = Array.IndexOf(parameters, matching);
            Arguments[index] = variable;

            return true;
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            var parameters = Method.GetParameters().ToArray();
            for (var i = 0; i < parameters.Length; i++)
            {
                if (Arguments[i] != null)
                {
                    continue;
                }

                var param = parameters[i];
                Arguments[i] = findVariable(param, chain);
            }

            foreach (var variable in Arguments)
            {
                yield return variable;
            }

            if (Method.IsStatic || IsLocal) yield break;

            if (Target == null)
            {
                Target = chain.FindVariable(HandlerType);
            }

            yield return Target;
        }


        private bool shouldAssignVariableToReturnValue(GeneratedMethod method)
        {
            if (ReturnVariable == null) return false;

            if (IsAsync && method.AsyncMode == AsyncMode.ReturnFromLastNode)
            {
                return false;
            }

            return true;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            if (CommentText.IsNotEmpty())
            {
                writer.WriteComment(CommentText);
            }
            
            var invokeMethod = invocationCode();

            var returnValue = "";

            if (IsAsync)
            {
                returnValue = method.AsyncMode == AsyncMode.ReturnFromLastNode ? "return " : "await ";
            }

            var isDisposable = false;
            if (shouldAssignVariableToReturnValue(method))
            {
#if !NET461
                returnValue = ReturnVariable.VariableType.IsValueTuple() ? $"{ReturnVariable.Usage} = {returnValue}" : $"var {ReturnVariable.Usage} = {returnValue}";
                #else
                returnValue = $"var {ReturnVariable.Usage} = {returnValue}";
#endif
                
                
                isDisposable = ReturnVariable.VariableType.CanBeCastTo<IDisposable>();
            }

            if (isDisposable && DisposalMode == DisposalMode.UsingBlock)
            {
                writer.UsingBlock($"{returnValue}{invokeMethod}", w => Next?.GenerateCode(method, writer));
            }
            else
            {
                writer.Write($"{returnValue}{invokeMethod};");

                Next?.GenerateCode(method, writer);
            }


        }

        private string invocationCode()
        {
            var methodName = Method.Name;
            if (Method.IsGenericMethod)
            {
                methodName += $"<{Method.GetGenericArguments().Select(x => x.FullName).Join(", ")}>";
            }

            var callingCode = $"{methodName}({Arguments.Select(x => x.ArgumentDeclaration).Join(", ")})";
            var target = determineTarget();
            var invokeMethod = $"{target}{callingCode}";
            return invokeMethod;
        }

        /// <summary>
        /// Code to invoke the method without any assignment to a variable
        /// </summary>
        /// <returns></returns>
        public string InvocationCode()
        {
            return IsAsync ? "await " + invocationCode() : invocationCode();
        }

        /// <summary>
        /// Code to invoke the method and set a variable to the returned value
        /// </summary>
        /// <returns></returns>
        public string AssignmentCode()
        {
            if (ReturnVariable == null)
            {
                throw new InvalidOperationException($"Method {this} does not have a return value");
            }

            return IsAsync
                ? $"var {ReturnVariable.Usage} = await {InvocationCode()}"
                : $"var {ReturnVariable.Usage} = {InvocationCode()}";

        }

        private string determineTarget()
        {
            if (IsLocal) return string.Empty;

            var target = Method.IsStatic
                ? HandlerType.FullNameInCode()
                : Target.Usage;

            return target + ".";
        }


        public override bool CanReturnTask()
        {
            return IsAsync;
        }

        public override string ToString()
        {
            return $"{nameof(HandlerType)}: {HandlerType}, {nameof(Method)}: {Method}";
        }
    }
}
