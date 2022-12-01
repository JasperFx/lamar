using System;
using Xunit;

namespace Lamar.Testing.Bugs;

public class Bug_137_stack_overflow_through_auto_discovery_of_services
{
    [Fact]
    public void fails_with_good_exception()
    {
        var container = Container.Empty();

        Exception<InvalidOperationException>.ShouldBeThrownBy(() => { container.GetInstance<Concrete1>(); });
    }

    public class Concrete1
    {
        public Concrete1(Concrete2 c2)
        {
        }
    }

    public class Concrete2
    {
        public Concrete2(Concrete3 c3)
        {
        }
    }

    public class Concrete3
    {
        public Concrete3(Concrete1 c1)
        {
        }
    }
}