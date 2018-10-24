using System;
using System.Collections.Generic;
using System.Linq;
using Lamar.Codegen;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using Lamar.Compilation;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;
using Lamar.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.IoC.Enumerables
{
    public class ListInstance<T> : GeneratedInstance
    {
        private Instance[] _elements;
        
        public ListInstance(Type serviceType) : base(serviceType, typeof(List<T>), ServiceLifetime.Transient)
        {
            Name = Variable.DefaultArgName(typeof(List<T>));
        }

        public override Frame CreateBuildFrame()
        {
            var variables = new ResolverVariables();
            var elements = _elements.Select(x => variables.Resolve(x, BuildMode.Dependency)).ToArray();
            variables.MakeNamesUnique();
            
            return new ListAssignmentFrame<T>(this, elements)
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
            
            return new ListAssignmentFrame<T>(this, elements).Variable;
        }

        protected internal override IEnumerable<Instance> createPlan(ServiceGraph services)
        {
            _elements = services.FindAll(typeof(T));
            return _elements;
        }

        public override object QuickResolve(Scope scope)
        {
            return _elements.Select(x => x.QuickResolve(scope).As<T>()).ToList();
        }

        public override string ToString()
        {
            return $"List of all {typeof(T).NameInCode()}";
        }
    }
}