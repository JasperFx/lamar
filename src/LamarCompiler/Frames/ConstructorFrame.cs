using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    }
    
    public class ConstructorFrame : SyncFrame
    {
        public ConstructorFrame(ConstructorInfo ctor) : this(ctor.DeclaringType, ctor)
        {

        }

        public ConstructorFrame(Type builtType, ConstructorInfo ctor)
        {
            BuiltType = builtType;
            Ctor = ctor ?? throw new ArgumentNullException(nameof(ctor));
            Parameters = new Variable[ctor.GetParameters().Length];
            
            // Need a declared variable?
        }

        public Type BuiltType { get; }
        public ConstructorInfo Ctor { get; }

        public Variable[] Parameters { get; }
        
        public Variable Result { get; private set; }

        public IList<Frame> ActivatorFrames { get; } = new List<Frame>();

        public ConstructorCallMode Mode { get; set; } = ConstructorCallMode.Variable;
        
        public IList<SetterArg> Setters { get; } = new List<SetterArg>();
        
        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            throw new NotImplementedException();
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