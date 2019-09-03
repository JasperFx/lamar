using Xunit;

namespace Lamar.Testing.IoC
{
    public class one_instance_across_multiple_interfaces
    {
        //SAMPLE: inverse-registration
        [Fact]
        public void when_singleton_both_interfaces_give_same_instance()
        {
            var container = new Container(services =>
            {
                services.Use<Implementation>()
                    .Singleton()
                    .For<IServiceA>()
                    .For<IServiceB>();
            });

            var instanceA = container.GetInstance<IServiceA>();
            var instanceB = container.GetInstance<IServiceB>();

            Assert.Same(instanceA, instanceB);
        }

        //ENDSAMPLE

        [Fact]
        public void when_transient_both_interfaces_give_new_instance()
        {
            var container = new Container(services =>
            {
                services.Use<Implementation>()
                    .Transient()
                    .For<IServiceA>()
                    .For<IServiceB>();
            });

            var instanceA = container.GetInstance<IServiceA>();
            var instanceB = container.GetInstance<IServiceB>();

            Assert.NotSame(instanceA, instanceB);
        }

        [Fact]
        public void when_scoped_instance_is_consistent_with_scope()
        {
            var container = new Container(services =>
            {
                services.Use<Implementation>()
                    .Scoped()
                    .For<IServiceA>()
                    .For<IServiceB>();
            });

            var instanceA = container.GetInstance<IServiceA>();
            var instanceB = container.GetInstance<IServiceB>();

            var scope1 = container.GetNestedContainer();

            var instanceA1 = scope1.GetInstance<IServiceA>();
            var instanceB1 = scope1.GetInstance<IServiceB>();

            var scope2 = container.GetNestedContainer();

            var instanceA2 = scope2.GetInstance<IServiceA>();
            var instanceB2 = scope2.GetInstance<IServiceB>();

            Assert.Same(instanceA, instanceB);
            Assert.Same(instanceA1, instanceB1);
            Assert.Same(instanceA2, instanceB2);

            Assert.NotSame(instanceA, instanceA1);
            Assert.NotSame(instanceA, instanceA2);
            Assert.NotSame(instanceA1, instanceA2);

            Assert.NotSame(instanceB, instanceB1);
            Assert.NotSame(instanceB, instanceB2);
            Assert.NotSame(instanceB1, instanceB2);
        }

        [Fact]
        public void when_named_instance_is_consistent_with_name()
        {
            var container = new Container(services =>
            {
                services.Use<Implementation>()
                    .Named("Group1")
                    .For<IServiceA>()
                    .For<IServiceB>();

                services.Use<Implementation>()
                    .Named("Group2")
                    .For<IServiceA>()
                    .For<IServiceB>();
            });

            var instanceA1 = container.GetInstance<IServiceA>("Group1");
            var instanceB1 = container.GetInstance<IServiceB>("Group1");

            var instanceA2 = container.GetInstance<IServiceA>("Group2");
            var instanceB2 = container.GetInstance<IServiceB>("Group2");

            Assert.Same(instanceA1, instanceB1);
            Assert.Same(instanceA2, instanceB2);

            Assert.NotSame(instanceA1, instanceA2);
            Assert.NotSame(instanceB1, instanceB2);
        }

        private interface IServiceA
        {
            void MethodA();
        }

        private interface IServiceB
        {
            void MethodB();
        }

        private class Implementation : IServiceA, IServiceB
        {
            public void MethodA()
            {
            }

            public void MethodB()
            {
            }
        }
    }
}