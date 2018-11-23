using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using LamarCompiler.Model;
using LamarCompiler.Util;

namespace LamarCompiler.Frames
{
    public enum ConstructorCallMode
    {
        Variable,
        ReturnValue,
        UsingNestedVariable
    }

    public class SetterArg
    {
        public SetterArg(string propertyName, Variable variable)
        {
            PropertyName = propertyName;
            Variable = variable;
        }

        public SetterArg(string propertyName, Type propertyType)
        {
            PropertyName = propertyName;
            PropertyType = propertyType;
        }

        public SetterArg(PropertyInfo property)
        {
            PropertyName = property.Name;
            PropertyType = property.PropertyType;
        }
        
        public SetterArg(PropertyInfo property, Variable variable)
        {
            PropertyName = property.Name;
            PropertyType = property.PropertyType;
            Variable = variable;
        }

        public string PropertyName { get; private set; }
        public Variable Variable { get; private set; }
        public Type PropertyType { get; private set; }

        public string Assignment()
        {
            return $"{PropertyName} = {Variable.Usage}";
        }


        internal void FindVariable(IMethodVariables chain)
        {
            if (Variable == null)
            {
                Variable = chain.FindVariable(PropertyType);
            }
        }
    }
    
    public class ConstructorFrame : SyncFrame
    {
        public ConstructorFrame(ConstructorInfo ctor) : this(ctor.DeclaringType, ctor)
        {

        }

        public ConstructorFrame(Type builtType, ConstructorInfo ctor) 
        {
            Ctor = ctor ?? throw new ArgumentNullException(nameof(ctor));
            Parameters = new Variable[ctor.GetParameters().Length];
            
            
            BuiltType = builtType;
            Variable = new Variable(BuiltType, this);
        }

        public Type BuiltType { get;  }
        
        public Type DeclaredType { get; set; }
        
        public ConstructorInfo Ctor { get; }

        public Variable[] Parameters { get; }
        
        public IList<Frame> ActivatorFrames { get; } = new List<Frame>();

        public ConstructorCallMode Mode { get; set; } = ConstructorCallMode.Variable;
        
        public IList<SetterArg> Setters { get; } = new List<SetterArg>();
        
        /// <summary>
        /// The variable set by invoking this frame. 
        /// </summary>
        public Variable Variable { get;}
        
        
        
        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            switch (Mode)
            {
                case ConstructorCallMode.Variable:
                    writer.Write(Declaration() + ";");
                    Next?.GenerateCode(method, writer);
                    break;
                    
                case ConstructorCallMode.ReturnValue:
                    writer.Write($"return {Invocation()};");
                    Next?.GenerateCode(method, writer);
                    break;
                    
                case ConstructorCallMode.UsingNestedVariable:
                    writer.UsingBlock(Declaration(), w => Next?.GenerateCode(method, w));
                    break;
            }
        }

        public string Declaration()
        {
            return DeclaredType == null 
                ? $"var {Variable.Usage} = {Invocation()}" 
                : $"{DeclaredType.FullNameInCode()} {Variable.Usage} = {Invocation()}";
        }

        public string Invocation()
        {
            var invocation = $"new {BuiltType.FullNameInCode()}({Parameters.Select(x => x.Usage).Join(", ")})";
            if (Setters.Any())
            {
                invocation += $"{{{Setters.Select(x => x.Assignment()).Join(", ")}}}";
            }

            return invocation;
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            var parameters = Ctor.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                
                if (Parameters[i] == null)
                {
                    var parameter = parameters[i];
                    Parameters[i] = chain.FindVariable(parameter.ParameterType);
                }
            }

            foreach (var parameter in Parameters)
            {
                yield return parameter;
            }

            foreach (var setter in Setters)
            {
                setter.FindVariable(chain);
            }

            foreach (var setter in Setters)
            {
                yield return setter.Variable;
            }

            foreach (var frame in ActivatorFrames)
            {
                foreach (var variable in frame.FindVariables(chain))
                {
                    yield return variable;
                }
            }
        }
    }

    public class ConstructorFrame<T> : ConstructorFrame
    {
        public ConstructorFrame(ConstructorInfo ctor) : base(typeof(T), ctor)
        {
        }

        public ConstructorFrame(Expression<Func<T>> expression) : base(typeof(T), ConstructorFinderVisitor<T>.Find(expression))
        {
        }

        public void Set(Expression<Func<T, object>> expression, Variable variable = null)
        {
            var property = ReflectionHelper.GetProperty(expression);
            var setter = new SetterArg(property, variable);
            
            Setters.Add(setter);
        }
    }
}