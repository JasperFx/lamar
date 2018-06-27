using System;
using System.Collections.Generic;
using Lamar.Codegen;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using Lamar.Compilation;
using Lamar.IoC;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;
using Lamar.IoC.Resolvers;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.Testing.IoC.Acceptance
{
    public class custom_instance_usage
    {
        
    }

    public class SessionInstance : GeneratedInstance
    {
        private Instance _store;

        public SessionInstance() : base(typeof(ISession), typeof(ISession), ServiceLifetime.Scoped)
        {
        }

        protected override IEnumerable<Instance> createPlan(ServiceGraph services)
        {
            _store = services.FindDefault(typeof(IStore));
            yield return _store;
        }


        public override Frame CreateBuildFrame()
        {
            throw new NotImplementedException();
        }

        protected override Variable generateVariableForBuilding(ResolverVariables variables, BuildMode mode, bool isRoot)
        {
            throw new NotImplementedException();
        }
    }

    public class OpenSessionFrame : SyncFrame
    {
        public OpenSessionFrame(SessionInstance instance, Variable store)
        {
            Variable = new ServiceVariable(instance, this);
            uses.Add(store);
        }
        
        public Variable Variable { get; }

        public bool ReturnCreated { get; set; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            throw new NotImplementedException();
        }
    }


    
    public interface ISession{}
    public class Session : ISession {}

    public interface IStore
    {
        ISession Open();
    }

    public class Store : IStore
    {
        public ISession Open()
        {
            return new Session();
        }
    }
}