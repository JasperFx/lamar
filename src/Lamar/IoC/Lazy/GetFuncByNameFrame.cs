using System;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;
using LamarCompiler;
using LamarCompiler.Frames;
using LamarCompiler.Model;

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