using System;
using System.Linq;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Lamar.Testing.IoC;

public class DecoratorPolicyTests
{
    [Fact]
    public void cannot_wrap_with_interface()
    {
        Exception<InvalidOperationException>.ShouldBeThrownBy(() => { new DecoratorPolicy<ISomething, ISomething>(); });

        Exception<InvalidOperationException>.ShouldBeThrownBy(() =>
        {
            new DecoratorPolicy<ISomething, ISomething2>();
        });
    }

    [Fact]
    public void cannot_wrap_with_abstract_class()
    {
        Exception<InvalidOperationException>.ShouldBeThrownBy(() =>
        {
            new DecoratorPolicy<ISomething, SomethingBase>();
        });
    }


    [Fact]
    public void does_not_let_you_use_a_class_with_no_inner_ctor_arg()
    {
        Exception<InvalidOperationException>.ShouldBeThrownBy(() =>
        {
            new DecoratorPolicy<ISomething, InvalidWrapper>();
        });
    }

    [Fact]
    public void wrap_negative()
    {
        var policy = new DecoratorPolicy<ISomething, WrappedThing>();

        var instance = new ObjectInstance(GetType(), this);

        policy.TryWrap(instance, out var wrapped).ShouldBeFalse();

        wrapped.ShouldBeNull();
    }

    [Fact]
    public void wrap_positive()
    {
        var policy = new DecoratorPolicy<ISomething, WrappedThing>();

        var instance = new ConstructorInstance(typeof(ISomething), typeof(InnerThing), ServiceLifetime.Transient);

        policy.TryWrap(instance, out var wrapped).ShouldBeTrue();
        var configured = wrapped.ShouldBeOfType<ConstructorInstance>();
        configured.ServiceType.ShouldBe(typeof(ISomething));
        configured.ImplementationType.ShouldBe(typeof(WrappedThing));


        configured.InlineDependencies.Single().ShouldBeSameAs(instance);
    }


    public interface ISomething
    {
    }

    public interface ISomething2 : ISomething
    {
    }

    public abstract class SomethingBase : ISomething
    {
    }

    public class InnerThing : ISomething
    {
    }

    public class WrappedThing : ISomething
    {
        public WrappedThing(ISomething something)
        {
        }
    }

    public class InvalidWrapper : ISomething
    {
        // no public ctor that accepts ISomething
    }
}