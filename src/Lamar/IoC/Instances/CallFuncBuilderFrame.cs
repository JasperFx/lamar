using System;
using System.Collections.Generic;
using System.Linq;
using Lamar.Codegen;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using Lamar.Compilation;
using Lamar.IoC.Frames;
using Lamar.IoC.Setters;
using Lamar.Util;

namespace Lamar.IoC.Instances
{
    public class CallFuncBuilderFrame : SyncFrame
    {
        private readonly Variable _func;
        private readonly Variable[] _arguments;
        private Variable _scope;

        public CallFuncBuilderFrame(ConstructorInstance instance, DisposeTracking disposal, Variable func,
            Variable[] arguments)
        {
            // HACK, and I hate this but it's necessary.
            for (int i = 0; i < arguments.Length; i++)
            {
                if (arguments[i] is OptionalArgumentVariable)
                {
                    arguments[i] = ((OptionalArgumentVariable) arguments[i]).Inner;
                }
            }
            
            Disposal = disposal;
            _func = func;
            _arguments = arguments;
            Variable = new ServiceVariable(instance, this);
        }

        public ServiceVariable Variable { get; }
        public DisposeTracking Disposal { get; }
        public bool ReturnCreated { get; set; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            var arguments = _arguments.Select(x => x.Usage).Join(", ");

            if (ReturnCreated)
            {
                writer.Write($"return {_func.Usage}({arguments});");
                Next?.GenerateCode(method, writer);
                return;
            }
            
            var declaration = $"var {Variable.Usage} = {_func.Usage}({arguments})";

            switch (Disposal)
            {
                case DisposeTracking.None:
                    writer.Write(declaration + ";");
                    Next?.GenerateCode(method, writer);
                    break;
                    
                case DisposeTracking.WithUsing:
                    if (Next is ConstructorFrame && Next.As<ConstructorFrame>().Disposal == DisposeTracking.WithUsing)
                    {
                        writer.Write($"using ({declaration})");
                        Next?.GenerateCode(method, writer);
                    }
                    else
                    {
                        writer.UsingBlock(declaration, w => Next?.GenerateCode(method, w));
                    }

                    break;
                    
                case DisposeTracking.RegisterWithScope:
                    writer.Write(declaration + ";");
                    writer.Write($"{_scope.Usage}.{nameof(Scope.Disposables)}.Add({Variable.Usage});");
                    Next?.GenerateCode(method, writer);
                    break;
                   
            }
            
            
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            foreach (var argument in _arguments)
            {
                yield return argument;
            }

            yield return _func;
            
            if (Disposal == DisposeTracking.RegisterWithScope)
            {
                _scope = chain.FindVariable(typeof(Scope));
                yield return _scope;
            }
        }
    }
}