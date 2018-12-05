using System;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Lamar.Testing.Bugs
{
    public class Bug_108_ServiceGraph_should_not_be_disposed_by_nested_container
    {
        [Fact]
        public void will_not_dispose_from_the_nested()
        {
            var guy = new DisposableOnceGuy();

            var container = Container.For(x => x.AddSingleton(guy));

            using (var nested = container.GetNestedContainer())
            {
                nested.GetInstance<DisposableOnceGuy>().ShouldBeSameAs(guy);
            }
            
            guy.WasDisposed.ShouldBeFalse();
            
            container.Dispose();
        }

        public class DisposableOnceGuy : IDisposable
        {
            public void Dispose()
            {
                if (WasDisposed) throw new InvalidOperationException();
                
                WasDisposed = true;
            }

            public bool WasDisposed { get; set; }
        }
    }
}