using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lamar.IoC.Frames;
using Lamar.IoC.Setters;
using LamarCompiler;
using LamarCompiler.Frames;
using LamarCompiler.Model;
using LamarCompiler.Util;
using SetterArg = Lamar.IoC.Setters.SetterArg;

namespace Lamar.IoC.Instances
{
    public class ConstructorFrame : SyncFrame
    {
        private readonly Variable[] _arguments;
        private readonly SetterArg[] _setterParameters;
        private Variable _scope;
        private readonly Type _implementationType;

        public ConstructorFrame(ConstructorInstance instance, DisposeTracking disposal, Variable[] arguments,
            SetterArg[] setterParameters)
        {
            Disposal = disposal;
            _arguments = arguments;
            _setterParameters = setterParameters;
            Variable = new ServiceVariable(instance, this);
            _implementationType = instance.ImplementationType;
        }
        
        public ServiceVariable Variable { get; }

        public DisposeTracking Disposal { get; }
        public bool ReturnCreated { get; set; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            var arguments = _arguments.Select(x => x.Usage).Join(", ");
            var implementationTypeName = _implementationType.FullNameInCode();


            if (ReturnCreated)
            {
                writer.Write($"return new {implementationTypeName}({arguments});");
                
                Next?.GenerateCode(method, writer);
                return;
            }
            
            var declaration = $"var {Variable.Usage} = new {implementationTypeName}({arguments})";

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
                    writer.Write($"{_scope.Usage}.{nameof(Scope.TryAddDisposable)}({Variable.Usage});");
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

            if (Disposal == DisposeTracking.RegisterWithScope)
            {
                _scope = chain.FindVariable(typeof(Scope));
                yield return _scope;
            }
            
            // TODO -- also go through the activator frames!
        }

        public override string ToString()
        {
            return $"new {_implementationType.NameInCode()}({_arguments.Select(x => x.VariableType.NameInCode()).Join(", ")})";
        }

        public class StandinMethodVariables : IMethodVariables
        {
            private readonly Variable _current;
            private readonly IMethodVariables _inner;

            public StandinMethodVariables(Variable current, IMethodVariables inner)
            {
                _current = current;
                _inner = inner;
            }

            public Variable FindVariable(Type type)
            {
                return type == _current.VariableType ? _current : _inner.FindVariable(type);
            }

            public Variable FindVariableByName(Type dependency, string name)
            {
                return _inner.FindVariableByName(dependency, name);
            }

            public bool TryFindVariableByName(Type dependency, string name, out Variable variable)
            {
                return _inner.TryFindVariableByName(dependency, name, out variable);
            }

            public Variable TryFindVariable(Type type, VariableSource source)
            {
                return _inner.TryFindVariable(type, source);
            }
        }
    }
}