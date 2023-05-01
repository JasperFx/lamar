using System;

namespace Lamar;

public interface IActivationInterceptor<T>
{
    T Intercept(Type serviceType, T instance, IServiceContext scope);
}