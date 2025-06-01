using System;
using System.Linq.Expressions;
using JasperFx.CodeGeneration.Expressions;
using JasperFx.CodeGeneration.Model;
using JasperFx.Core.Reflection;
using Lamar.IoC.Frames;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.IoC.Instances;

public class NullInstance : Instance
{
    public NullInstance(Type serviceType) : base(serviceType, serviceType, ServiceLifetime.Transient)
    {
        Hash = GetHashCode();
    }

    public override Func<Scope, object> ToResolver(Scope topScope)
    {
        return s => null;
    }

    public override object Resolve(Scope scope)
    {
        return null;
    }


    public override Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot)
    {
        return new DefaultVariable(ServiceType);
    }
}

public class DefaultVariable : Variable
{
    public DefaultVariable(Type variableType) : base(variableType, $"default({variableType.FullNameInCode()})")
    {
    }

    public override Expression ToVariableExpression(LambdaDefinition definition)
    {
        return Expression.Default(VariableType);
    }
}

public class NullVariable : Variable
{
    public NullVariable(Type variableType) : base(variableType, "null")
    {
    }

    public override void OverrideName(string variableName)
    {
        // nothing
    }
}