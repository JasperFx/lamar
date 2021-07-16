using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lamar.IoC.Instances;

namespace Lamar
{
    [LamarIgnore]
    public class ActivationInterceptorPolicy<T> : IDecoratorPolicy
    {
        private readonly IActivationInterceptor<T> _interceptor;

        public ActivationInterceptorPolicy(IActivationInterceptor<T> interceptor)
        {
            _interceptor = interceptor;
        }

        public bool TryWrap(Instance inner, out Instance wrapped)
        {
            if (inner.ServiceType.IsInterface && typeof(T).IsAssignableFrom(inner.ServiceType))
            {
                wrapped = new InterceptingInstance<T>(inner, _interceptor);
                return true;
            }

            wrapped = null;
            return false;
        }
    }
}
