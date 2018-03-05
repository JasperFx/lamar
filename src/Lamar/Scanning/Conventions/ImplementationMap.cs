using System;
using System.Linq;
using Lamar.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.Scanning.Conventions
{
    public class ImplementationMap : IRegistrationConvention
    {
        public void ScanTypes(TypeSet types, IServiceCollection services)
        {
            var interfaces = types.FindTypes(TypeClassification.Interfaces | TypeClassification.Closed).Where(x => x != typeof(IDisposable));
            var concretes = types.FindTypes(TypeClassification.Concretes | TypeClassification.Closed).Where(x => x.GetConstructors().Any()).ToArray();

            interfaces.Each(@interface =>
            {
                var implementors = concretes.Where(x => x.CanBeCastTo(@interface)).ToArray();
                if (implementors.Count() == 1)
                {
                    services.AddType(@interface, implementors.Single());
                }
            });
        }

        public override string ToString()
        {
            return "Register any single implementation of any interface against that interface";
        }
    }
}
