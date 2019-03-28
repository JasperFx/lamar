using System;
using System.Linq.Expressions;
using System.Reflection;

namespace LamarCodeGeneration.Util
{
    internal class ConstructorFinderVisitor<T> : ExpressionVisitorBase
    {
        private readonly Type _type;
        private ConstructorInfo _constructor;

        public ConstructorFinderVisitor(Type type)
        {
            _type = type;
        }

        public ConstructorInfo Constructor => _constructor;

        protected override NewExpression VisitNew(NewExpression nex)
        {
            if (nex.Type == _type)
            {
                _constructor = nex.Constructor;
            }

            return base.VisitNew(nex);
        }

        public static ConstructorInfo Find(Expression<Func<T>> expression)
        {
            var finder = new ConstructorFinderVisitor<T>(typeof(T));
            finder.Visit(expression);

            return finder.Constructor;
        }
    }
}