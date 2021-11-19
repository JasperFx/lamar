using Lamar;
using Widget.Core.Interfaces;
using Widget.Instance;

namespace Widget.Registration
{
    public class BugWidgetRegistration : ServiceRegistry
    {
        public BugWidgetRegistration()
        {
            For<IBugWidget>().Use<BugWidget>();
        }

    }
}
