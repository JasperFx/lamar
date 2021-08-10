using Shouldly;
using Xunit;

namespace StructureMap.Testing.Acceptance
{
    public class forwarding
    {
        #region sample_forwarding-sample-types
        public interface IWriter { }

        public interface IReader { }

        public class StatefulCache : IReader, IWriter
        {
        }

        #endregion

        #region sample_forwarding-in-action
        [Fact]
        public void stateful_cache_serves_multiple_interfaces()
        {
            var container = new Container(_ =>
            {
                // Let's make StatefulCache a SingletonThing in the container
                _.ForConcreteType<StatefulCache>().Configure.Singleton();

                _.Forward<StatefulCache, IReader>();
                _.Forward<StatefulCache, IWriter>();
            });

            container.GetInstance<IReader>().ShouldBeOfType<StatefulCache>();
            container.GetInstance<IWriter>().ShouldBeOfType<StatefulCache>();
        }

        #endregion

        #region sample_forward-without-forward
        [Fact]
        public void equivalent()
        {
            var container = new Container(_ =>
            {
                // Let's make StatefulCache a SingletonThing in the container
                _.ForConcreteType<StatefulCache>().Configure.Singleton();

                _.For<IReader>().Use(c => c.GetInstance<StatefulCache>());
                _.For<IWriter>().Use(c => c.GetInstance<StatefulCache>());
            });

            container.GetInstance<IReader>().ShouldBeOfType<StatefulCache>();
            container.GetInstance<IWriter>().ShouldBeOfType<StatefulCache>();
        }

        #endregion
    }
}