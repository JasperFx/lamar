using System;
using Lamar;
using Widget.Core.Interfaces;
using Widget.Aspect.Logger;

namespace Widget.Registration
{
    public class BugWidgetAspectRegistration : ServiceRegistry
    {
        public BugWidgetAspectRegistration()
        {
            For<IBugWidget>().DecorateAllWith<WidgetBugAspectLogger>();
        }
    }
}
