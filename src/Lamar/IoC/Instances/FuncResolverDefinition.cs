using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JasperFx.Core.Reflection;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Expressions;
using JasperFx.CodeGeneration.Model;
using JasperFx.CodeGeneration.Util;

namespace Lamar.IoC.Instances
{
    public class FuncResolverDefinition : IGeneratedType, IGeneratedMethod
    {
        private readonly Scope _scope;
        private readonly Argument _scopeArgument;
        private readonly GenerationRules _rules;
        private readonly FramesCollection _frames;

        public FuncResolverDefinition(GeneratedInstance instance, Scope scope, GenerationRules rules = null)
        {
            _scope = scope;
            _rules = rules ?? new GenerationRules("Jasper.Generated");
            _scopeArgument = new Argument(typeof(Scope), "scope");
            var frame = instance.CreateBuildFrame();

            _frames = new FramesCollection(null) { frame };
        }

        IList<Setter> IGeneratedType.Setters { get; } = new List<Setter>();
        IList<InjectedField> IGeneratedType.AllInjectedFields { get; } = new List<InjectedField>();
        GenerationRules IGeneratedType.Rules => _rules;
        FramesCollection IGeneratedMethod.Frames => _frames;
        public Argument[] Arguments => new Argument[] { _scopeArgument };
        IList<Variable> IGeneratedMethod.DerivedVariables { get; } = new List<Variable>();
        IList<IVariableSource> IGeneratedMethod.Sources { get; } = new List<IVariableSource>();

        public Func<Scope, object> BuildResolver()
        {
            var arranger = new MethodFrameArranger(this, this);
            arranger.Arrange(out var mode, out var top);

            var definition = new LambdaDefinition
            {
                Context = _scope
            };
            var scope = definition.RegisterExpression(_scopeArgument).As<ParameterExpression>();
            definition.Arguments = new[] { scope };

            if (top is not IResolverFrame)
            {
                throw new InvalidOperationException($"Frame type {top} does not implement {nameof(IResolverFrame)}");
            }

            var current = top;
            do
            {
                var frame = current.As<IResolverFrame>();
                frame.WriteExpressions(definition);
                current = current.Next;
            } while (current is not null);

            return definition.Compile<Func<Scope, object>>();
        }
    }
}