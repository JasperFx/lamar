using System;
using System.Collections.Generic;
using LamarCompiler;
using LamarCompiler.Frames;
using LamarCompiler.Model;

namespace Lamar.IoC.Resolvers
{
    public class CastScopeFrame : SyncFrame
    {
        private Variable _scope;

        public CastScopeFrame(Type interfaceType)
        {
            Variable = new Variable(interfaceType, this);
        }
        
        public Variable Variable { get; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"var {Variable.Usage} = ({Variable.VariableType.FullNameInCode()}) {_scope.Usage};");
            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            _scope = chain.FindVariable(typeof(Scope));
            yield return _scope;
        }
    }
}