using System;
using System.Linq.Expressions;
using System.Reflection;

namespace JasperFx.CodeGeneration.Util;

internal class ConstructorFinderVisitor<T> : ExpressionVisitor
{
    private readonly Type _type;

    public ConstructorFinderVisitor(Type type)
    {
        _type = type;
    }

    public ConstructorInfo Constructor { get; private set; }

    protected override Expression VisitNew(NewExpression node)
    {
        if (node.Type == _type)
        {
            Constructor = node.Constructor;
        }

        return base.VisitNew(node);
    }

    public static ConstructorInfo Find(Expression<Func<T>> expression)
    {
        var finder = new ConstructorFinderVisitor<T>(typeof(T));
        finder.Visit(expression);

        return finder.Constructor;
    }
}