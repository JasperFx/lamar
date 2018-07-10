using Lamar.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace Lamar.Testing.AspNetCoreIntegration
{
    public class using_options_policy
    {
        [Fact]
        public void can_resolve_optionssnapshot()
        {
            var container = new Container(_ =>
            {
                _.AddOptions();
                _.Policies.Add<OptionsPolicy>();
                _.For<IOptionsSnapshot<SomethingOptions>>().Use<OptionsManager<SomethingOptions>>();
            });
            
            container.GetInstance<IOptionsSnapshot<SomethingOptions>>()
                .ShouldNotBeNull();
        }

        [Fact]
        public void use_as_argument_to_ctor()
        {
            var container = new Container(_ =>
            {
                _.AddOptions();
                _.Policies.Add<OptionsPolicy>();
            });
            
            container.GetInstance<SnapshotHolder>()
                .Snapshot.ShouldNotBeNull();
        }
    }

    public class SnapshotHolder
    {
        public IOptionsSnapshot<SomethingOptions> Snapshot { get; }

        public SnapshotHolder(IOptionsSnapshot<SomethingOptions> snapshot)
        {
            Snapshot = snapshot;
        }
    }

    public class SomethingOptions
    {
        
    }
}