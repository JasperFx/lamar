using System;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance;

public class do_not_blow_up_with_bi_directional_dependencies
{
    [Fact]
    public void do_not_blow_up_with_a_stack_overflow_problem()
    {
        var ex =
            Exception<InvalidOperationException>.ShouldBeThrownBy(() =>
            {
                var container = new Container(x =>
                {
                    x.For<IBiView>().Use<BiView>();
                    x.For<IBiPresenter>().Use<BiPresenter>();

                    x.For<IBiGrandparent>().Use<BiGrandparent>();
                    x.For<IBiHolder>().Use<BiHolder>();
                    x.For<IBiLeaf>().Use<BiLeaf>();
                });
            });

        ex.Message.ShouldContain("Bi-directional dependencies detected");
    }

    [Fact]
    public void do_not_blow_up_with_a_stack_overflow_problem_2()
    {
        var ex =
            Exception<InvalidOperationException>.ShouldBeThrownBy(() =>
            {
                var container = new Container(x =>
                {
                    x.For<IBiView>().Use<BiView>();
                    x.For<IBiPresenter>().Use<BiPresenter>();

                    x.For<IBiGrandparent>().Use<BiGrandparent>();
                    x.For<IBiHolder>().Use<BiHolder>();
                    x.For<IBiLeaf>().Use<BiLeaf>();
                });

                container.GetInstance<IBiHolder>();
            });

        ex.Message.ShouldContain("Bi-directional dependencies detected");
    }
}

public interface IBiHolder
{
}

public interface IBiGrandparent
{
}

public interface IBiLeaf
{
}

#region sample_using-LamarIgnore

// This attribute causes the type scanning to ignore this type
[LamarIgnore]
public class BiHolder : IBiHolder
{
    public BiHolder(IBiGrandparent grandparent)
    {
    }
}

#endregion

[LamarIgnore]
public class BiGrandparent : IBiGrandparent
{
    public BiGrandparent(IBiLeaf leaf)
    {
    }
}

public class BiLeaf : IBiLeaf
{
    public BiLeaf(IBiHolder holder)
    {
    }
}

public interface IBiView
{
}

public interface IBiPresenter
{
}

[LamarIgnore]
public class BiView : IBiView
{
    public BiView(IBiPresenter presenter)
    {
        Presenter = presenter;
    }

    public IBiPresenter Presenter { get; }
}

public class BiPresenter : IBiPresenter
{
    public BiPresenter(IBiView view)
    {
        View = view;
    }

    public IBiView View { get; }
}