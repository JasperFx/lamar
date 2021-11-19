using System;
using Widget.Core.Interfaces;

namespace Widget.Instance
{
    public class BugWidget : IBugWidget
    {
        public bool IFixedWidget()
        {
            Console.WriteLine("I Fixed the Widget");
            return true;
        }
    }
}
