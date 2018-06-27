using System;
using System.IO;
using System.Linq;
using Baseline;
using Lamar.IoC.Exports;
using Lamar.IoC.Instances;
using Lamar.IoC.Resolvers;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Exports
{
    public class using_cached_set
    {
        [Fact]
        public void try_it_out()
        {
            var container = new Container(x =>
            {
                x.AddTransient<IWidget, AWidget>();
            });


            var path = Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "Internal", "Resolvers");
            container.Model.ExportResolverCode<WidgetCachedSet>(path);
            
            

        }

        [Fact]
        public void try_to_load_resolvers()
        {
            var @set = new WidgetCachedSet();
            var resolvers = @set.LoadResolvers();
            
            resolvers.Values.Single().CanBeCastTo<IResolver>().ShouldBeTrue();
        }

        [Fact]
        public void use_it_for_realsies()
        {
            var container = new Container(x =>
            {
                x.AddTransient<IWidget, AWidget>();
                x.Policies.Add<WidgetCachedSet>();
            });

            container.GetInstance<IWidget>()
                .ShouldBeOfType<AWidget>();
        }
        
        public class WidgetCachedSet : CachedResolverSet
        {
            public override bool Include(GeneratedInstance instance)
            {
                return true;
            }
        }
    }
}