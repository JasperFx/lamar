using System;
using Microsoft.Extensions.DependencyInjection;
using LamarCompiler.Util;

namespace Lamar.IoC.Resolvers
{
    public class ScopedLambdaResolver<TContainer, T> : ScopedResolver<T>
    {
        private readonly Func<TContainer, T> _builder;

        public ScopedLambdaResolver(Func<TContainer, T> builder)
        {
            _builder = builder;
        }

        public override T Build(Scope scope)
        {
            return _builder(scope.As<TContainer>());
        }
    }
}