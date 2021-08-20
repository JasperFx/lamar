using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lamar.IoC;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC
{
    public class ActivationInterceptionTests
    {
        private class TestInterceptor<T> : IActivationInterceptor<T> 
        {
            public int InterceptCount { get; private set; }

            public T Intercept(Type serviceType, T instance, IServiceContext scope)
            {
                ++InterceptCount;
                return instance;
            }
        }

        [Fact]
        public void ActivationIsIntercepted()
        {
            var interceptor = new TestInterceptor<IWidget>();

            var container = new Container(c =>
            {
                c.Policies.Add(new ActivationInterceptorPolicy<IWidget>(interceptor));

                c.For<IWidget>().Use<AWidget>();
            });

            var instance = container.GetInstance<IWidget>();
            
            interceptor.InterceptCount.ShouldBe(1);
        }
    }
}
