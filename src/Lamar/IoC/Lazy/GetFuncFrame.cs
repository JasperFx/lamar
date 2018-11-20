using System;
using System.Linq;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;
using LamarCompiler;
using LamarCompiler.Frames;
using LamarCompiler.Model;

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