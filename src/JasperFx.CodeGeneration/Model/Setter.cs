using System;
using System.Linq.Expressions;
using JasperFx.CodeGeneration.Expressions;

namespace JasperFx.CodeGeneration.Model;

public enum SetterType
{
    ReadWrite,
    ReadOnly,
    Constant,
    StaticReadOnly
}

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

    /// <summary>
    ///     Value to be set upon creating an instance of the class
    /// </summary>
    public object InitialValue { get; set; }

    public Variable ReadOnlyValue { get; set; }

    public SetterType Type { get; set; } = SetterType.ReadWrite;

    public static Setter ReadOnly(string name, Variable value)
    {
        return new Setter(value.VariableType, name)
        {
            Type = SetterType.ReadOnly,
            ReadOnlyValue = value
        };
    }

    public static Setter StaticReadOnly(string name, Variable value)
    {
        return new Setter(value.VariableType, name)
        {
            Type = SetterType.StaticReadOnly,
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

            case SetterType.StaticReadOnly:
                return $"public static {VariableType.FullNameInCode()} {PropName} {{get;}} = {ReadOnlyValue.Usage};";
        }

        throw new NotSupportedException();
    }

    public void SetInitialValue(object @object)
    {
        if (InitialValue == null || Type != SetterType.ReadWrite)
        {
            return;
        }

        var property = @object.GetType().GetProperty(Usage);
        property.SetValue(@object, InitialValue);
    }

    public override Expression ToVariableExpression(LambdaDefinition definition)
    {
        if (InitialValue != null)
        {
            return Expression.Constant(InitialValue, VariableType);
        }

        throw new InvalidOperationException("No initial value to create an expression");
    }
}