using System.Collections.Generic;
using System.Threading.Tasks;
using Shouldly;
using Widget.Aspect.Logger;
using Widget.Core.Interfaces;
using Widget.Instance;
using Widget.Registration;
using Xunit;

namespace Lamar.Testing.Bugs;

public class Bug_124_decorator_compliation_error_when_applying_decorator_from_one_library_registration_in_other_library
{
    [Fact]
    public void it_should_not_blow_up()
    {
        var container = new Container(_ =>
        {
            _.Scan(x =>
            {
                //x.TheCallingAssembly();
                // since we have other dlls with registries classes
                // make sure to load them for discovery...
                //x.AssembliesFromApplicationBaseDirectory();
                x.AssemblyContainingType<IBugWidget>();
                x.AssemblyContainingType<BugWidget>();
                x.AssemblyContainingType<WidgetBugAspectLogger>();
                x.AssemblyContainingType<BugWidgetAspectRegistration>();
                x.LookForRegistries();
            });
        });

        void tryToResolveAll()
        {
            container.GetInstance<IBugWidget>().ShouldNotBeNull();
        }

        var list = new List<Task>();

        for (var i = 0; i < 10; i++)
        {
            list.Add(Task.Factory.StartNew(tryToResolveAll));
        }

        Task.WaitAll(list.ToArray());
    }
}