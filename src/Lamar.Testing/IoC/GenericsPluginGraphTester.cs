﻿using System;
using Lamar.IoC.Instances;
using Shouldly;
using Xunit;

namespace Lamar.Testing.IoC;

public class GenericsPluginGraphTester
{
    private void assertCanBeCast(Type pluginType, Type TPluggedType)
    {
        GenericsPluginGraph.CanBeCast(pluginType, TPluggedType).ShouldBeTrue();
    }

    private void assertCanNotBeCast(Type pluginType, Type TPluggedType)
    {
        GenericsPluginGraph.CanBeCast(pluginType, TPluggedType).ShouldBeFalse();
    }

    [Fact]
    public void BuildAnInstanceManagerFromTemplatedPluginFamily()
    {
        var container = new Container(x =>
        {
            x.For(typeof(IGenericService<>)).Add(typeof(SecondGenericService<>)).Named("Second");
            x.For(typeof(IGenericService<>)).Add(typeof(ThirdGenericService<>)).Named("Third");
            x.For(typeof(IGenericService<>)).Use(typeof(GenericService<>)).Named("Default");
        });

        var intService = container.GetInstance<IGenericService<int>>().ShouldBeOfType<GenericService<int>>();
        intService.GetT().ShouldBe(typeof(int));

        container.GetInstance<IGenericService<int>>("Second").ShouldBeOfType<SecondGenericService<int>>();

        var stringService =
            (GenericService<string>)container.GetInstance<IGenericService<string>>();
        stringService.GetT().ShouldBe(typeof(string));
    }


    [Fact]
    public void Check_the_generic_plugin_family_expression()
    {
        var container =
            new Container(
                r =>
                {
                    r.For(typeof(IGenericService<>)).Use(
                        typeof(GenericService<>));
                });

        container.GetInstance<IGenericService<string>>().ShouldBeOfType(typeof(GenericService<string>));
    }

    [Fact]
    public void checking_can_be_cast()
    {
        assertCanBeCast(typeof(IOpenType<>), typeof(OpenType<>));
    }

    [Fact]
    public void DirectImplementationOfInterfaceCanBeCast()
    {
        assertCanBeCast(typeof(IGenericService<>), typeof(GenericService<>));
        assertCanNotBeCast(typeof(IGenericService<>), typeof(SpecificService<>));
    }

    [Fact]
    public void DirectInheritanceOfAbstractClassCanBeCast()
    {
        assertCanBeCast(typeof(BaseSpecificService<>), typeof(SpecificService<>));
    }

    [Fact]
    public void ImplementationOfInterfaceFromBaseType()
    {
        assertCanBeCast(typeof(ISomething<>), typeof(SpecificService<>));
    }

    [Fact]
    public void RecursiveImplementation()
    {
        assertCanBeCast(typeof(ISomething<>), typeof(SpecificService<>));
        assertCanBeCast(typeof(ISomething<>), typeof(GrandChildSpecificService<>));
    }

    [Fact]
    public void RecursiveInheritance()
    {
        assertCanBeCast(typeof(BaseSpecificService<>), typeof(ChildSpecificService<>));
        assertCanBeCast(typeof(BaseSpecificService<>), typeof(GrandChildSpecificService<>));
    }
}

public interface IGenericService<T>
{
}

public class GenericService<T> : IGenericService<T>
{
    public Type GetT()
    {
        return typeof(T);
    }
}

public class SecondGenericService<T> : IGenericService<T>
{
}

public class ThirdGenericService<T> : IGenericService<T>
{
}

public interface ISomething<T>
{
}

public interface ISomething2<T>
{
}

public interface ISomething3<T>
{
}

public abstract class BaseSpecificService<T> : ISomething<T>
{
}

public class SpecificService<T> : BaseSpecificService<T>
{
}

public class ChildSpecificService<T> : SpecificService<T>
{
}

public class GrandChildSpecificService<T> : ChildSpecificService<T>
{
}

public interface IGenericService3<T, U, V>
{
}

public class GenericService3<T, U, V> : IGenericService3<T, U, V>
{
    public Type GetT()
    {
        return typeof(T);
    }
}

public class SecondGenericService3<T, U, V> : IGenericService3<T, U, V>
{
}

public class ThirdGenericService3<T, U, V> : IGenericService3<T, U, V>
{
}

public interface IOpenType<T>
{
}

public class OpenType<T> : IOpenType<T>
{
}