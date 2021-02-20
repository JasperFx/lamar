using Lamar.Diagnostics;
using Lamar.IoC;
using Shouldly;
using Xunit;

namespace Lamar.Testing.Bugs
{
    public class Bug_275_misleading_exception
    {
        public class Foo : IFoo
        {
            public Foo(IRandomDependency dependency)
            {
            }
        }

        public interface IFoo
        {
        }

        public interface IRandomDependency
        {
        }

        [Fact]
        public void try_it_out()
        {
            // var helpful = new Container(services =>
            // {
            //     services.For<IFoo>().Use<Foo>(); //expected Exception
            // });
            //
            // helpful.GetInstance<IFoo>();
            //
            // var helpful2 = new Container(services =>
            // {
            //     services.For<IFoo>().Use<Foo>().Transient();
            // });
            // helpful2.GetInstance<IFoo>();

            var notHelpful = new Container(services =>
            {
                services.For<IFoo>().Use<Foo>().Singleton();
            });

            var ex = Should.Throw<LamarException>(() => notHelpful.GetInstance<IFoo>());
            ex.Message.ShouldContain("Cannot fill the dependencies of any of the public constructors");
        }
    }
}