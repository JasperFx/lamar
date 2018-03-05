using System;
using System.Linq;
using Lamar.Codegen;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;

namespace Lamar.IoC.Lazy
{
    public class GetFuncFrame : TemplateFrame
    {
        private object _scope;
        private readonly Type _serviceType;

        public GetFuncFrame(Instance instance, Type innerType)
        {
            _serviceType = innerType;
            Variable = new ServiceVariable(instance, this);
        }
        
        public Variable Variable { get; }

        protected override string Template()
        {
            _scope = Arg<Scope>();
            return $"System.Func<{_serviceType.FullNameInCode()}> {Variable.Usage} = () => {_scope}.{nameof(IContainer.GetInstance)}<{_serviceType.FullNameInCode()}>();";
        }
    }
}