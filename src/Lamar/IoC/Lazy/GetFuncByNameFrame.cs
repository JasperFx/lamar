using System;
using Lamar.Codegen;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;

namespace Lamar.IoC.Lazy
{
    public class GetFuncByNameFrame : TemplateFrame
    {
        private object _scope;
        private readonly Type _serviceType;

        public GetFuncByNameFrame(Instance instance, Type innerType)
        {
            _serviceType = innerType;
            Variable = new ServiceVariable(instance, this);
        }
        
        public Variable Variable { get; }

        protected override string Template()
        {
            _scope = Arg<Scope>();
            return $"System.Func<string, {_serviceType.FullNameInCode()}> {Variable.Usage} = name => {_scope}.{nameof(IContainer.GetInstance)}<{_serviceType.FullNameInCode()}>(name);";
        }
    }
}