using System;
using Widget.Core.Interfaces;

namespace Widget.Aspect.Logger
{
    public class WidgetBugAspectLogger : IBugWidget
    {
        private readonly IBugWidget _bugWidget;
        public WidgetBugAspectLogger(IBugWidget bugWidget)
        {
            _bugWidget = bugWidget;
        }

        public bool IFixedWidget()
        {
            Console.WriteLine("Log: Bug aspect widget...");
            if(_bugWidget != null)
            {
                _bugWidget.IFixedWidget();
            }
            return true;
        }
    }
}
