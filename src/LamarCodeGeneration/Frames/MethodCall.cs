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

    public enum ReturnAction
    {
        /// <summary>
        /// The value built by the MethodCall should be like 'return Method()'
        /// </summary>
        Return,
        
        /// <summary>
        /// The value built by the MethodCall should be assigned to the ReturnVariable
        /// like 'x = Method();'
        /// </summary>
        Assign,
        
        /// <summary>
        /// The value built by the MethodCall should be assigned to the ReturnVariable
        /// like 'var x = Method();`
        /// </summary>
        Initialize
    }
    
    public class MethodCall : Frame
    {
        public Dictionary<Type, Type> Aliases { get; } = new Dictionary<Type, Type>();

        public Type HandlerType { get; }
        public MethodInfo Method { get; }


        public void AssignResultTo(Variable variable)
        {
            ReturnVariable = variable;
            ReturnAction = ReturnAction.Assign;
        }
        
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

            ReturnType = correctedReturnType(method.ReturnType);
            if (ReturnType != null)
            {
#if !NET461 && !NET48
                

                if (ReturnType.IsValueTuple())
                {
                    var values = ReturnType.GetGenericArguments().Select(x => new Variable(x, this)).ToArray();
                
                    ReturnVariable = new ValueTypeReturnVariable(ReturnType, values);
                }
                else
                {
#endif
                    var name = ReturnType.IsSimple() || ReturnType == typeof(object) || ReturnType == typeof(object[])
                        ? "result_of_" + method.Name
                        : Variable.DefaultArgName(ReturnType);
                    

                    ReturnVariable = new Variable(ReturnType, name, this); 
                    
#if !NET461 && !NET48
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

        public Type ReturnType { get; }

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
        
        /// <summary>
        /// How should the ReturnVariable handled within the generated code? Initialize is the default.
        /// </summary>
        public ReturnAction ReturnAction { get; set; } = ReturnAction.Initialize;

        public bool TrySetArgument(Variable variable)
        {
            var parameters = Method.GetParameters().Select(x => x.ParameterType).ToArray();
            if (parameters.Count(x => variable.VariableType == x) != 1) return false;
            
            var index = Array.IndexOf(parameters, variable.VariableType);
            Arguments[index] = variable;

            return true;
        }

        public bool TrySetArgument(string parameterName, Variable variable)
        {
            var parameters = Method.GetParameters().ToArray();
            var matching = parameters.FirstOrDefault(x =>
                variable.VariableType == x.ParameterType && x.Name == parameterName);

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


        private string returnActionCode(GeneratedMethod method)
        {
            if (IsAsync && method.AsyncMode == AsyncMode.ReturnFromLastNode)
            {
                return "return ";
            }
            
            if (ReturnVariable == null)
            {
                return string.Empty;
            }

#if !NET4x
            if (ReturnVariable.VariableType.IsValueTuple())
            {
                return $"{ReturnVariable.Usage} = ";
            }
#endif


            switch (ReturnAction)
            {
                case ReturnAction.Initialize:
                    return $"var {ReturnVariable.Usage} = ";
                case ReturnAction.Assign:
                    return $"{ReturnVariable.Usage} = ";
                case ReturnAction.Return:
                    return "return ";
            }
            
            throw new ArgumentOutOfRangeException();
        }

        private bool shouldWriteInUsingBlock(GeneratedMethod method)
        {
            if (ReturnVariable == null) return false;

            if (IsAsync && method.AsyncMode == AsyncMode.ReturnFromLastNode) return false;

            return ReturnVariable.VariableType.CanBeCastTo<IDisposable>() && DisposalMode == DisposalMode.UsingBlock;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            if (CommentText.IsNotEmpty())
            {
                writer.WriteComment(CommentText);
            }
            
            var invokeMethod = InvocationCode(method);

            if (shouldWriteInUsingBlock(method))
            {
                writer.UsingBlock($"{returnActionCode(method)}{invokeMethod}", w => Next?.GenerateCode(method, writer));
            }
            else
            {
                writer.Write($"{returnActionCode(method)}{invokeMethod};");

                // This is just to make the generated code a little
                // easier to read
                if (CommentText.IsNotEmpty())
                {
                    writer.BlankLine();
                }
                
                Next?.GenerateCode(method, writer);
            }


        }

        private string invocationCode()
        {
            var methodName = Method.Name;
            if (Method.IsGenericMethod)
            {
                methodName += $"<{Method.GetGenericArguments().Select(x => x.FullNameInCode()).Join(", ")}>";
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
        public string InvocationCode(GeneratedMethod method)
        {
            var code = invocationCode();
            


            if (IsAsync && method.AsyncMode != AsyncMode.ReturnFromLastNode)
            {
#if NET461 || NET48
                code = $"await {code}.ConfigureAwait(false)";
                #else
                code = $"await {code}";
#endif
            }

            return code;
        }

        /// <summary>
        /// Code to invoke the method and set a variable to the returned value
        /// </summary>
        /// <returns></returns>
        public string AssignmentCode(GeneratedMethod method)
        {
            if (ReturnVariable == null)
            {
                throw new InvalidOperationException($"Method {this} does not have a return value");
            }

            return IsAsync
#if NET461 || NET48
                ? $"var {ReturnVariable.Usage} = await {InvocationCode(method)}.ConfigureAwait(false)"
                : $"var {ReturnVariable.Usage} = {InvocationCode(method)}.ConfigureAwait(false)";
#else
                ? $"var {ReturnVariable.Usage} = await {InvocationCode(method)}"
                : $"var {ReturnVariable.Usage} = {InvocationCode(method)}";
#endif


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
