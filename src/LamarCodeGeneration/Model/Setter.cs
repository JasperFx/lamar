using System;
using System.Linq.Expressions;
using LamarCodeGeneration.Expressions;

namespace LamarCodeGeneration.Model
{
    public enum SetterType
    {
        ReadWrite,
        ReadOnly,
        Constant
    }
    
    public class Setter : Variable
    {
        public static Setter ReadOnly(string name, Variable value)
        {
            return new Setter(value.VariableType, name)
            {
                Type = SetterType.ReadOnly,
                ReadOnlyValue = value
            };
        }

        public static Setter Constant(string name, Variable value)
        {
            return new Setter(value.VariableType, name)
            {
                Type = SetterType.Constant,
                ReadOnlyValue = value
            };
        }
        
        public Setter(Type variableType) : base(variableType)
        {
        }

        public Setter(Type variableType, string name) : base(variableType, name)
        {
            PropName = name;
        }

        public string PropName { get; set; }

        public virtual void WriteDeclaration(ISourceWriter writer)
        {
            writer.Write(ToDeclaration());
        }

        public string ToDeclaration()
        {
            switch (Type)
            {
                case SetterType.ReadWrite:
                    return $"public {VariableType.FullNameInCode()} {PropName} {{get; set;}}";
                
                case SetterType.Constant:
                    return $"public const {VariableType.FullNameInCode()} {PropName} = {ReadOnlyValue.Usage};";
                
                case SetterType.ReadOnly:
                    return $"public {VariableType.FullNameInCode()} {PropName} {{get;}} = {ReadOnlyValue.Usage};";
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Value to be set upon creating an instance of the class
        /// </summary>
        public object InitialValue { get; set; }
        
        public Variable ReadOnlyValue { get; set; }

        public SetterType Type { get; set; } = SetterType.ReadWrite;

        public void SetInitialValue(object @object)
        {
            if (InitialValue == null || Type != SetterType.ReadWrite) return;            
            
            var property = @object.GetType().GetProperty(Usage);
            property.SetValue(@object, InitialValue);
        }

        public override Expression ToVariableExpression(LambdaDefinition definition)
        {
            if (InitialValue != null) return Expression.Constant(InitialValue, VariableType);
            
            throw new InvalidOperationException("No initial value to create an expression");
        }
    }
}