using System;
using Lamar.IoC.Instances;

namespace Lamar.IoC.Activation
{
    internal class InterceptorPolicy<T> : IDecoratorPolicy
    {
        private readonly Func<IServiceContext, T, T> _interceptor;

        public InterceptorPolicy(Func<IServiceContext, T, T> interceptor)
        {
            _interceptor = interceptor;
        }

        public virtual bool TestInstance(Instance inner)
        {
            return inner.ServiceType == typeof(T);
        }

        public bool TryWrap(Instance inner, out Instance wrapped)
        {
            if (TestInstance(inner))
            {
                wrapped = new InterceptingInstance<T>(_interceptor, inner);

                return true;
            }

            wrapped = null;
            return false;
        }

    }
}