using Lamar.Scanning.Conventions;
using Lamar.Testing.IoC.Acceptance;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Scanning.Conventions
{
    public class DefaultConventionScannerTests
    {
        [Fact]
        public void the_default_overwrite_behavior_is_if_new()
        {
            new DefaultConventionScanner()
                .Overwrites.ShouldBe(OverwriteBehavior.NewType);
        }
        
        [Fact]
        public void should_add_always()
        {
            var scanner = new DefaultConventionScanner
            {
                Overwrites = OverwriteBehavior.Always
            };
            
            var services = new ServiceRegistry();

            scanner.ShouldAdd(services, typeof(IWidget), typeof(AWidget)).ShouldBeTrue();

            services.AddTransient<IWidget, BWidget>();
            scanner.ShouldAdd(services, typeof(IWidget), typeof(AWidget)).ShouldBeTrue();
            
            services.AddTransient<IWidget>(x => new AWidget());
            scanner.ShouldAdd(services, typeof(IWidget), typeof(AWidget)).ShouldBeTrue();

            services.AddTransient<IWidget, AWidget>();
            scanner.ShouldAdd(services, typeof(IWidget), typeof(AWidget)).ShouldBeTrue();
        }
        
        [Fact]
        public void should_add_with_overwrite_never()
        {
            var scanner = new DefaultConventionScanner
            {
                Overwrites = OverwriteBehavior.Never
            };
            
            var services = new ServiceRegistry();

            scanner.ShouldAdd(services, typeof(IWidget), typeof(AWidget)).ShouldBeTrue();

            services.AddTransient<IWidget, BWidget>();
            scanner.ShouldAdd(services, typeof(IWidget), typeof(AWidget)).ShouldBeFalse();
            
            services = new ServiceRegistry();
            services.AddTransient<IWidget>(x => new AWidget());
            scanner.ShouldAdd(services, typeof(IWidget), typeof(AWidget)).ShouldBeFalse();

            services = new ServiceRegistry();
            services.AddTransient<IWidget, AWidget>();
            scanner.ShouldAdd(services, typeof(IWidget), typeof(AWidget)).ShouldBeFalse();
        }
        
        [Fact]
        public void should_add_with_overwrite_if_newer()
        {
            var scanner = new DefaultConventionScanner
            {
                Overwrites = OverwriteBehavior.NewType
            };
            
            var services = new ServiceRegistry();

            scanner.ShouldAdd(services, typeof(IWidget), typeof(AWidget)).ShouldBeTrue();

            services.AddTransient<IWidget, BWidget>();
            scanner.ShouldAdd(services, typeof(IWidget), typeof(AWidget)).ShouldBeTrue();
            
            services = new ServiceRegistry();
            services.AddTransient<IWidget>(x => new AWidget());
            // Can't tell that it's an AWidget, so add
            scanner.ShouldAdd(services, typeof(IWidget), typeof(AWidget)).ShouldBeTrue();

            services = new ServiceRegistry();
            services.AddTransient<IWidget, AWidget>();
            scanner.ShouldAdd(services, typeof(IWidget), typeof(AWidget)).ShouldBeFalse();
        }
        
        
    }
}