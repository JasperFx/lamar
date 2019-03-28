using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LamarCompiler.Expressions;
using LamarCompiler.Frames;
using LamarCompiler.Util;

namespace LamarCompiler.Model
{
    public class Variable
    {
        public static Variable[] VariablesForProperties<T>(string rootArgName)
        {
            return typeof(T).GetTypeInfo().GetProperties().Where(x => x.CanRead)
                .Select(x => new Variable(x.PropertyType, $"{rootArgName}.{x.Name}"))
                .ToArray();
        }

        public static Variable For<T>(string variableName = null)
        {
            return new Variable(typeof(T), variableName ?? DefaultArgName(typeof(T)));
        }


        public static string DefaultArgName(Type argType)
        {
            if (argType.IsArray)
            {
                return DefaultArgName(argType.GetElementType()) + "Array";
            }

            if (argType.IsEnumerable())
            {
                var argPrefix = DefaultArgName(argType.DetermineElementType());
                var suffix = argType.GetGenericTypeDefinition().Name.Split('`').First();

                return argPrefix + suffix;
            }

            var parts = argType.Name.SplitPascalCase().Split(' ');
            if (argType.GetTypeInfo().IsInterface && parts.First() == "I")
            {
                parts = parts.Skip(1).ToArray();
            }

            var raw = (parts.First().ToLower() + parts.Skip(1).Join("")).Split('`').First();
            return raw;
        }

        public static string DefaultArgName<T>()
        {
            return DefaultArgName(typeof(T));
        }

        private Frame _frame;

        public Frame Creator
        {
            get => _frame;
            protected set
            {
                _frame = value;
                Creator?.creates.Fill(this);
            }
        }
        public Type VariableType { get; }
        public virtual string Usage { get; protected set; }

        public virtual string ArgumentDeclaration => Usage;
        
        /// <summary>
        /// Used to smuggle additional information about a variable
        /// </summary>
        public Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();

        /// <summary>
        /// On rare occasions you may need to override the variable name
        /// </summary>
        /// <param name="variableName"></param>
        public virtual void OverrideName(string variableName)
        {
            Usage = variableName;
        }

        /// <summary>
        /// On rare occasions you may need to override the variable type
        /// </summary>
        /// <param name="variableType"></param>
        public void OverrideType(Type variableType)
        {
            
        }

        /// <summary>
        /// Marks other variables that this variable depends on
        /// </summary>
        public IList<Variable> Dependencies { get; } = new List<Variable>();

        public Variable(Type variableType) : this(variableType, DefaultArgName(variableType))
        {

        }

        public Variable(Type variableType, string usage)
        {
            VariableType = variableType;
            Usage = usage;
        }

        public Variable(Type variableType, string usage, Frame creator) : this(variableType, usage)
        {
            if (creator != null)
            {
                Creator = creator;

                Creator.creates.Fill(this);
            }
        }

        public Variable(Type variableType, Frame creator) : this(variableType, DefaultArgName(variableType), creator)
        {

        }

        public override string ToString()
        {
            return $"{nameof(VariableType)}: {VariableType}, {nameof(Usage)}: {Usage}";
        }

        protected bool Equals(Variable other)
        {
            return VariableType == other.VariableType && string.Equals(Usage, other.Usage);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Variable) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((VariableType != null ? VariableType.GetHashCode() : 0) * 397) ^ (Usage != null ? Usage.GetHashCode() : 0);
            }
        }

        public virtual Expression ToVariableExpression(LambdaDefinition definition)
        {
            return Expression.Variable(VariableType, Usage);
        }
    }
}
