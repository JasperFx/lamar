using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lamar.IoC;

namespace Lamar
{
    public interface IActivationInterceptor<T>
    {
        T Intercept(Type serviceType, T instance, Scope scope);
    }
}
