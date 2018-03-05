using System;
using Lamar.Codegen.Variables;

namespace Lamar.Codegen.ServiceLocation
{
    public class ServiceVariableSource : IVariableSource
    {
        private readonly IMethodVariables _method;
        private readonly ServiceGraph _services;

        public ServiceVariableSource(IMethodVariables method, ServiceGraph services)
        {
            _method = method ?? throw new ArgumentNullException(nameof(method));
            _services = services;
        }

        public bool Matches(Type type)
        {
            // TODO -- will need to redo this one
            throw new NotImplementedException();
            //return _services.CanResolve(type);
        }

        public Variable Create(Type type)
        {
            return new ServiceResolutionFrame(type).Service;
        }
    }
}
