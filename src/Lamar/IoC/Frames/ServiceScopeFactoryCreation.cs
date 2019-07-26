using System;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.IoC.Frames
{
    public class ServiceScopeFactoryCreation : SyncFrame
    {
        private readonly Variable _factory;
        private readonly Variable _scope;

        public ServiceScopeFactoryCreation()
        {

            _scope = new Variable(typeof(IServiceScope), this);
            Provider = new Variable(typeof(IServiceProvider), this);
            Wraps = true;
        }

        public ServiceScopeFactoryCreation(Variable factory) : this()
        {
            _factory = factory;
            uses.Add(factory);
        }

        public Variable Provider { get; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.UsingBlock($"var {_scope.Usage} = {_factory.Usage}.{nameof(IServiceScopeFactory.CreateScope)}()", w =>
            {
                w.Write($"var {Provider.Usage} = {_scope.Usage}.{nameof(IServiceScope.ServiceProvider)};");
                Next?.GenerateCode(method, w);
            });
        }

    }
}
