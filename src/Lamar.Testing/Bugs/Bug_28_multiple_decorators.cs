using Shouldly;
using Xunit;

namespace Lamar.Testing.Bugs
{
    public class Bug_28_multiple_decorators
    {
        [Fact]
        public void can_build_and_use()
        {
            var container = Container.For(_ =>
            {
                _.For<ILogger>().Use<BasicLogger>();
                _.For<ILogger>().DecorateAllWith<FancyLogger>();
                _.For<ILogger>().DecorateAllWith<OtherLogger>();
            });

            var logger = container.GetInstance<ILogger>();
            logger.ShouldBeOfType<OtherLogger>().Inner
                .ShouldBeOfType<FancyLogger>().Inner
                .ShouldBeOfType<BasicLogger>();
        }
    }

    public interface ILogger
    {
        void Log(string message);
    }

    public class BasicLogger : ILogger
    {
        public void Log(string message)
        {
            
        }
    }

    public class FancyLogger : ILogger
    {
        public ILogger Inner { get; }

        public FancyLogger(ILogger inner)
        {
            Inner = inner;
        }

        public void Log(string message)
        {
            Inner.Log(message);
        }
    }

    public class OtherLogger : FancyLogger
    {
        public OtherLogger(ILogger inner) : base(inner)
        {
        }
    }
}
