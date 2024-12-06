using Lamar.IoC;
using Shouldly;
using Xunit;

namespace Lamar.Testing.IoC;

public class InstanceIdentifierTests
{
    [Fact]
    public void equals_uses_type_and_name()
    {
        var a = new InstanceIdentifier("foo", typeof(InstanceIdentifier));
        var b = new InstanceIdentifier("foo", typeof(InstanceIdentifier));

        a.ShouldBe(b);
    }
    
    [Fact]
    public void equals_uses_type()
    {
        var a = new InstanceIdentifier(default, typeof(InstanceIdentifier));
        var b = new InstanceIdentifier(default, typeof(InstanceIdentifier));

        a.ShouldBe(b);
    }

    [Fact]
    public void notequals_uses_type_and_name()
    {
        var a = new InstanceIdentifier("foo", typeof(InstanceIdentifier));
        var b = new InstanceIdentifier("bar", typeof(InstanceIdentifier));

        a.ShouldNotBe(b);
    }
    
    [Fact]
    public void notequals_uses_type()
    {
        var a = new InstanceIdentifier(default, typeof(InstanceIdentifier));
        var b = new InstanceIdentifier(default, typeof(InstanceIdentifierTests));

        a.ShouldNotBe(b);
    }
}