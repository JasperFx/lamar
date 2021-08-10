using Xunit;

namespace Lamar.Testing.IoC
{
    public class one_instance_across_multiple_interfaces
    {
        #region sample_inverse-registration
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

            instanceA.ShouldBeTheSameAs(instanceB);
        }

        #endregion

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

            instanceA.ShouldNotBeTheSameAs(instanceB);
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

            instanceA.ShouldBeTheSameAs(instanceB);
            instanceA1.ShouldBeTheSameAs(instanceB1);
            instanceA2.ShouldBeTheSameAs(instanceB2);

            instanceA.ShouldNotBeTheSameAs(instanceA1);
            instanceA.ShouldNotBeTheSameAs(instanceA2);
            instanceA1.ShouldNotBeTheSameAs(instanceA2);

            instanceB.ShouldNotBeTheSameAs(instanceB1);
            instanceB.ShouldNotBeTheSameAs(instanceB2);
            instanceB1.ShouldNotBeTheSameAs(instanceB2);
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

            instanceA1.ShouldBeTheSameAs(instanceB1);
            instanceA2.ShouldBeTheSameAs(instanceB2);

            instanceA1.ShouldNotBeTheSameAs(instanceA2);
            instanceB1.ShouldNotBeTheSameAs(instanceB2);
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