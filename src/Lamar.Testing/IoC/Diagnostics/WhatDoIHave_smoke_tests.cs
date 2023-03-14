using System;
using Microsoft.Extensions.DependencyInjection;
using StructureMap.Testing.Widget;
using Xunit;
using Xunit.Abstractions;

namespace Lamar.Testing.IoC.Diagnostics;

public class WhatDoIHave_Smoke_Tester
{
    private readonly ITestOutputHelper _output;

    public WhatDoIHave_Smoke_Tester(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void empty_container()
    {
        #region sample_whatdoihave-simple

        var container = Container.Empty();
        var report = container.WhatDoIHave();

        Console.WriteLine(report);

        #endregion
    }

    [Fact]
    public void display_one_service_for_an_interface()
    {
        #region sample_what_do_i_have_container

        var container = new Container(x =>
        {
            x.For<IEngine>().Use<Hemi>().Named("The Hemi");

            x.For<IEngine>().Add<VEight>().Singleton().Named("V8");
            x.For<IEngine>().Add<FourFiftyFour>();
            x.For<IEngine>().Add<StraightSix>().Scoped();

            x.For<IEngine>().Add(c => new Rotary()).Named("Rotary");
            x.For<IEngine>().Add(c => c.GetService<PluginElectric>());

            x.For<IEngine>().Add(new InlineFour());

            x.For<IEngine>().UseIfNone<VTwelve>();
        });

        #endregion

        #region sample_whatdoihave_everything

        Console.WriteLine(container.WhatDoIHave());

        #endregion

        _output.WriteLine(container.WhatDoIHave());
    }


    [Fact]
    public void display_one_service_for__a_nested_container()
    {
        var container = new Container(x =>
        {
            x.For<IEngine>().Use<Hemi>().Named("The Hemi");

            x.For<IEngine>().Add<VEight>().Singleton().Named("V8");
            x.For<IEngine>().Add<FourFiftyFour>();
            x.For<IEngine>().Add<StraightSix>().Scoped();

            x.For<IEngine>().Add(c => new Rotary()).Named("Rotary");
            x.For<IEngine>().Add(c => c.GetService<PluginElectric>());

            x.For<IEngine>().Add(new InlineFour());
        });

        Console.WriteLine(container.GetNestedContainer().WhatDoIHave());
    }


    [Fact]
    public void filter_by_assembly()
    {
        var container = new Container(x =>
        {
            x.For<IEngine>().Use<Hemi>().Named("The Hemi");

            x.For<IEngine>().Add<VEight>().Singleton().Named("V8");
            x.For<IEngine>().Add<FourFiftyFour>();
            x.For<IEngine>().Add<StraightSix>().Scoped();

            x.For<IEngine>().Add(c => new Rotary()).Named("Rotary");
            x.For<IEngine>().Add(c => c.GetService<PluginElectric>());

            x.For<IEngine>().Add(new InlineFour());

            x.For<IWidget>().Use<AWidget>();
        });

        #region sample_whatdoihave-assembly

        Console.WriteLine(container.WhatDoIHave(assembly: typeof(IWidget).Assembly));

        #endregion
    }

    [Fact]
    public void filtering_examples()
    {
        #region sample_whatdoihave-filtering

        var container = Container.Empty();

        // Filter by the Assembly of the Service Type
        var byAssembly = container.WhatDoIHave(assembly: typeof(IWidget).Assembly);

        // Only report on the specified Service Type
        var byServiceType = container.WhatDoIHave(typeof(IWidget));

        // Filter to Service Type's in the named namespace
        // The 'IsInNamespace' test will include child namespaces
        var byNamespace = container.WhatDoIHave(@namespace: "StructureMap.Testing.Widget");

        // Filter by a case insensitive string.Contains() match
        // against the Service Type name
        var byType = container.WhatDoIHave(typeName: "Widget");

        #endregion
    }

    [Fact]
    public void filter_by_plugin_type()
    {
        var container = new Container(x =>
        {
            x.For<IEngine>().Use<Hemi>().Named("The Hemi");

            x.For<IEngine>().Add<VEight>().Singleton().Named("V8");
            x.For<IEngine>().Add<FourFiftyFour>();
            x.For<IEngine>().Add<StraightSix>().Scoped();

            x.For<IEngine>().Add(c => new Rotary()).Named("Rotary");
            x.For<IEngine>().Add(c => c.GetService<PluginElectric>());

            x.For<IEngine>().Add(new InlineFour());

            x.For<IWidget>().Use<AWidget>();
        });

        #region sample_whatdoihave-plugintype

        Console.WriteLine(container.WhatDoIHave(typeof(IWidget)));

        #endregion
    }

    [Fact]
    public void filter_by_type_name()
    {
        var container = new Container(x =>
        {
            x.For<IEngine>().Use<Hemi>().Named("The Hemi");

            x.For<IEngine>().Add<VEight>().Singleton().Named("V8");
            x.For<IEngine>().Add<FourFiftyFour>();
            x.For<IEngine>().Add<StraightSix>().Scoped();

            x.For<IEngine>().Add(s => new Rotary()).Named("Rotary");
            x.For<IEngine>().Add(c => c.GetService<PluginElectric>());

            x.For<IEngine>().Add(new InlineFour());

            x.For<IWidget>().Use<AWidget>();

            x.For<AWidget>().Use<AWidget>();
        });

        #region sample_whatdoihave-type

        Console.WriteLine(container.WhatDoIHave(typeName: "Widget"));

        #endregion
    }


    [Fact]
    public void filter_by_namespace()
    {
        var container = new Container(x =>
        {
            x.For<IEngine>().Use<Hemi>().Named("The Hemi");

            x.For<IEngine>().Add<VEight>().Singleton().Named("V8");
            x.For<IEngine>().Add<FourFiftyFour>();
            x.For<IEngine>().Add<StraightSix>().Scoped();

            x.For<IEngine>().Add(c => new Rotary()).Named("Rotary");
            x.For<IEngine>().Add(c => c.GetService<PluginElectric>());

            x.For<IEngine>().Add(new InlineFour());

            x.For<IWidget>().Use<AWidget>();

            x.For<AWidget>().Use<AWidget>();
        });

        #region sample_whatdoihave-namespace

        Console.WriteLine(container.WhatDoIHave(@namespace: "System"));

        #endregion
    }
}

public interface IAutomobile
{
}

public interface IEngine
{
}

public class NamedEngine : IEngine
{
    public NamedEngine(string name)
    {
        Name = name;
    }

    public string Name { get; }
}

public class VEight : IEngine
{
}

public class StraightSix : IEngine
{
}

public class Hemi : IEngine
{
}

public class FourFiftyFour : IEngine
{
}

public class VTwelve : IEngine
{
}

public class Rotary : IEngine
{
}

public class PluginElectric : IEngine
{
}

public class InlineFour : IEngine
{
    public override string ToString()
    {
        return "I'm an inline 4!";
    }
}