using System;
using Lamar.Codegen;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;

namespace Lamar.IoC.Lazy
{
    public class GetLazyFrame : TemplateFrame
    {
        private object _scope;
        private readonly Type _serviceType;

        public GetLazyFrame(Instance instance, Type innerType)
        {
            _serviceType = innerType;
            Variable = new ServiceVariable(instance, this);
        }
        
        public Variable Variable { get; }

        protected override string Template()
        {
            _scope = Arg<Scope>();
            return $"var {Variable.Usage} = new System.Lazy<{_serviceType.FullNameInCode()}>(() => {_scope}.{nameof(IContainer.GetInstance)}<{_serviceType.FullNameInCode()}>());";
        }
    }
}