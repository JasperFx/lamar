﻿using StructureMap.Graph;
using StructureMap.Testing.GenericWidgets;
using StructureMap.Testing.Widget;
using System.Diagnostics;
using Xunit;

namespace StructureMap.Testing
{
    public class WhatDidIScan_smoke_tester
    {
        [Fact]
        public void what_did_i_scan_usage()
        {
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

            Debug.WriteLine(container.WhatDidIScan());
            #endregion
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
}