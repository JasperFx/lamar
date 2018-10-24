using System;
using Lamar.Codegen.Variables;
using Lamar.IoC;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lamar.Microsoft.DependencyInjection
{
    public class LoggerInstance<T> : Instance
    {
        private readonly object _locker = new object();
        
        public LoggerInstance() : base(typeof(ILogger<T>), typeof(Logger<T>), ServiceLifetime.Singleton)
        {
        }

        public override Func<Scope, object> ToResolver(Scope topScope)
        {
            return s => resolveFromRoot(topScope);
        }

        public override object Resolve(Scope scope)
        {
            return resolveFromRoot(scope.Root);
        }

        private object resolveFromRoot(Scope root)
        {
            if (tryGetService(root, out object service))
            {
                return service;
            }

            lock (_locker)
            {
                if (tryGetService(root, out service))
                {
                    return service;
                }
                
                var factory = root.GetInstance<ILoggerFactory>();
                var logger = new Logger<T>(factory);
                store(root, logger);

                return logger;
            }
        }

        public override Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot)
        {
            return new InjectedServiceField(this);
        }
    }
}