using System;
using System.Linq;
using Lamar.IoC;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;
using LamarCompiler.Model;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC
{
    public class ServiceRegistryTests
    {
        [Fact]
        public void can_add_custom_instance()
        {
            var registry = new ServiceRegistry();
            
            var custom = new SpecialInstance(typeof(IWidget), typeof(AWidget), ServiceLifetime.Scoped);

            registry.For<IWidget>().Use(custom);
            
            registry.Single().ImplementationInstance.ShouldBe(custom);
        }
    }

    public class SpecialInstance : Instance
    {
        public SpecialInstance(Type serviceType, Type implementationType, ServiceLifetime lifetime) : base(serviceType, implementationType, lifetime)
        {
        }

        public override Func<Scope, object> ToResolver(Scope topScope)
        {
            throw new NotImplementedException();
        }

        public override object Resolve(Scope scope)
        {
            throw new NotImplementedException();
        }

        public override Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot)
        {
            throw new NotImplementedException();
        }
    }
}