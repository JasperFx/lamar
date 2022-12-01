using System;
using System.Collections.Generic;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;

namespace Lamar.IoC.Frames
{
    public class NestedContainerCreation : AsyncFrame
    {
        public NestedContainerCreation()
        {
            Root = new InjectedField(typeof(IContainer), "rootContainer");
            Nested = new Variable(typeof(IContainer), "nestedContainer", this);
        }

        public Variable Root { get; }

        public Variable Nested { get; }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            yield return Root;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"await using var {Nested.Usage} = ({typeof(IContainer).FullNameInCode()})_rootContainer.{nameof(IContainer.GetNestedContainer)}();");
            Next?.GenerateCode(method, writer);
        }
    }

    public class GetInstanceFromNestedContainerFrame : SyncFrame
    {
        private readonly Variable _nested;

        public GetInstanceFromNestedContainerFrame(Variable nested, Type serviceType)
        {
            _nested = nested;
            uses.Add(_nested);
            
            Variable = new Variable(serviceType, this);
        }
        
        public Variable Variable { get; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"var {Variable.Usage} = {_nested.Usage}.{nameof(IContainer.GetInstance)}<{Variable.VariableType.FullNameInCode()}>();");
            Next?.GenerateCode(method, writer);
        }
    }
    
    
}