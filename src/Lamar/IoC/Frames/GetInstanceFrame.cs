using System.Collections.Generic;
using Lamar.Codegen;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using Lamar.Compilation;
using Lamar.IoC.Instances;

namespace Lamar.IoC.Frames
{
    
    
    public class GetInstanceFrame : SyncFrame
    {
        private Variable _scope;
        private readonly string _name;

        public GetInstanceFrame(Instance instance)
        {
            Variable = new ServiceVariable(instance, this, ServiceDeclaration.ServiceType);
            
            _name = instance.Name;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"var {Variable.Usage} = {_scope.Usage}.{nameof(Scope.GetInstance)}<{Variable.VariableType.FullNameInCode()}>(\"{_name}\");");
            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            _scope = chain.FindVariable(typeof(Scope));
            yield return _scope;
        }
        
        public ServiceVariable Variable { get; }
    }
}