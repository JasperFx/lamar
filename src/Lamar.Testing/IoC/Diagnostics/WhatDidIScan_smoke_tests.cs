using System;
using StructureMap.Testing.GenericWidgets;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Diagnostics;

public class WhatDidIScan_smoke_tester
{
    [Fact]
    public void what_did_i_scan_usage()
    {
        var c = new Container(x => x.For<IWidget>().Use<ColorWidget>());


        #region sample_whatdidiscan

        var container = new Container(_ =>
        {
            _.Scan(x =>
            {
                x.TheCallingAssembly();

                x.WithDefaultConventions();
                x.RegisterConcreteTypesAgainstTheFirstInterface();
                x.SingleImplementationsOfInterface();
            });

            _.Scan(x =>
            {
                // Give your scanning operation a descriptive name
                // to help the diagnostics to be more useful
                x.Description = "Second Scanner";

                x.AssembliesFromApplicationBaseDirectory(assem => assem.FullName.Contains("Widget"));
                x.ConnectImplementationsToTypesClosing(typeof(IService<>));
                x.AddAllTypesOf<IWidget>();
            });
        });

        Console.WriteLine(container.WhatDidIScan());

        #endregion
    }

    [Fact]
    public void what_did_i_scan_should_list_assemblies()
    {
        var c = new Container(x => x.For<IWidget>().Use<ColorWidget>());


        var container = new Container(_ =>
        {
            _.Scan(x =>
            {
                x.TheCallingAssembly();

                x.WithDefaultConventions();
                x.RegisterConcreteTypesAgainstTheFirstInterface();
                x.SingleImplementationsOfInterface();
            });

            _.Scan(x =>
            {
                // Give your scanning operation a descriptive name
                // to help the diagnostics to be more useful
                x.Description = "Second Scanner";

                x.AssembliesFromApplicationBaseDirectory(assem => assem.FullName.Contains("Widget"));
                x.ConnectImplementationsToTypesClosing(typeof(IService<>));
                x.AddAllTypesOf<IWidget>();
            });
        });

        var whatDidIScan = container.WhatDidIScan();
        whatDidIScan.ShouldContain("* StructureMap.Testing.GenericWidgets");
        whatDidIScan.ShouldContain("* StructureMap.Testing.Widget");
        whatDidIScan.ShouldContain("* StructureMap.Testing.Widget2");
        whatDidIScan.ShouldContain("* StructureMap.Testing.Widget3");
        whatDidIScan.ShouldContain("* StructureMap.Testing.Widget4");
        whatDidIScan.ShouldContain("* StructureMap.Testing.Widget5");

        Console.WriteLine(whatDidIScan);
    }
}

#region sample_whatdidiscan-result

/*
All Scanners
================================================================
Scanner #1
Assemblies
----------
* StructureMap.Testing, Version=4.0.0.51243, Culture=neutral, PublicKeyToken=null

Conventions
--------
* Default I[Name]/[Name] registration convention
* Register all concrete types against the first interface (if any) that they implement
* Register any single implementation of any interface against that interface

Second Scanner
Assemblies
----------
* StructureMap.Testing.GenericWidgets, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
* StructureMap.Testing.Widget, Version=4.0.0.51243, Culture=neutral, PublicKeyToken=null
* StructureMap.Testing.Widget2, Version=4.0.0.51243, Culture=neutral, PublicKeyToken=null
* StructureMap.Testing.Widget3, Version=4.0.0.51243, Culture=neutral, PublicKeyToken=null
* StructureMap.Testing.Widget4, Version=4.0.0.51243, Culture=neutral, PublicKeyToken=null
* StructureMap.Testing.Widget5, Version=4.0.0.51243, Culture=neutral, PublicKeyToken=null

Conventions
--------
* Connect all implementations of open generic type IService<T>
* Find and register all types implementing StructureMap.Testing.Widget.IWidget

*/

#endregion