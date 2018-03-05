using System;
using System.Collections.Generic;
using System.Linq;
using Lamar.Codegen;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using Lamar.Compilation;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;
using Lamar.IoC.Resolvers;
using Lamar.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.IoC.Enumerables
{
    public class ArrayInstance<T> : GeneratedInstance
    {
        private Instance[] _elements;

        public ArrayInstance(Type serviceType) : base(serviceType, typeof(T[]), ServiceLifetime.Transient)
        {
            Name = Variable.DefaultArgName<T[]>();
        }

        public override Frame CreateBuildFrame()
        {
            var variables = new ResolverVariables();
            var elements = _elements.Select(x => variables.Resolve(x, BuildMode.Dependency)).ToArray();
            
            return new ArrayAssignmentFrame<T>(this, elements)
            {
                ReturnCreated = true
            };
        }

        protected override Variable generateVariableForBuilding(ResolverVariables variables, BuildMode mode, bool isRoot)
        {
            // This is goofy, but if the current service is the top level root of the resolver
            // being created here, make the dependencies all be Dependency mode
            var dependencyMode = isRoot && mode == BuildMode.Build ? BuildMode.Dependency : mode;
            
            var elements = _elements.Select(x => variables.Resolve(x, dependencyMode)).ToArray();
            
            return new ArrayAssignmentFrame<T>(this, elements).Variable;
        }

        protected override IEnumerable<Instance> createPlan(ServiceGraph services)
        {
            _elements = services.FindAll(typeof(T));
            return _elements;
        }

        public override object QuickResolve(Scope scope)
        {
            return _elements.Select(x => x.QuickResolve(scope).As<T>()).ToArray();
        }
    }
}