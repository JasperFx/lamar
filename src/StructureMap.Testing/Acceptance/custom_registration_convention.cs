﻿using Shouldly;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;
using StructureMap.Graph.Scanning;
using System;
using System.Reflection;
using Xunit;

namespace StructureMap.Testing.Acceptance
{
    public class custom_registration_convention
    {
        #region sample_custom-registration-convention
        public interface IFoo
        {
        }

        public interface IBar
        {
        }

        public interface IBaz
        {
        }

        public class BusyGuy : IFoo, IBar, IBaz
        {
        }

        // Custom IRegistrationConvention
        public class AllInterfacesConvention : IRegistrationConvention
        {
            public void ScanTypes(TypeSet types, Registry registry)
            {
                // Only work on concrete types
                types.FindTypes(TypeClassification.Concretes | TypeClassification.Closed).Each(type =>
                {
                    // Register against all the interfaces implemented
                    // by this concrete class
                    type.GetInterfaces().Each(@interface => registry.For(@interface).Use(type));
                });
            }
        }

        [Fact]
        public void use_custom_registration_convention()
        {
            var container = new Container(_ =>
            {
                _.Scan(x =>
                {
                    // You're probably going to want to tightly filter
                    // the Type's that are applicable to avoid unwanted
                    // registrations
                    x.TheCallingAssembly();
                    x.IncludeNamespaceContainingType<BusyGuy>();

                    // Register the custom policy
                    x.Convention<AllInterfacesConvention>();
                });
            });

            container.GetInstance<IFoo>().ShouldBeOfType<BusyGuy>();
            container.GetInstance<IBar>().ShouldBeOfType<BusyGuy>();
            container.GetInstance<IBaz>().ShouldBeOfType<BusyGuy>();
        }

        #endregion
    }
}