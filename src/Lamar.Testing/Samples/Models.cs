namespace Lamar.Testing.Samples;

#region sample_foobar-model

public interface IBar
{
}

public class Bar : IBar
{
}

public interface IFoo
{
}

public class Foo : IFoo
{
    public Foo(IBar bar)
    {
        Bar = bar;
    }

    public IBar Bar { get; }
}

#endregion