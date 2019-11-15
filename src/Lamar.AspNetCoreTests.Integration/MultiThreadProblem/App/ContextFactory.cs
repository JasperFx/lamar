using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lamar.AspNetCoreTests.Integration.MultiThreadProblem.App
{
    public class ContextFactory : IContextFactory
    {
        private readonly Context _scopedContext;

        public ContextFactory(Context scopedContext)
        {
            _scopedContext = scopedContext;
        }

        public Context CreateNewContext()
        {
            var newContext = new Context(_scopedContext.SecondContext);
            return newContext;
        }
    }
}
