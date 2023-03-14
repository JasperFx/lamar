using System;
using System.Linq.Expressions;
using JasperFx.Core.Reflection;
using Lamar.IoC.Instances;
using Lamar.Scanning.Conventions;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Expressions;
using JasperFx.CodeGeneration.Model;
using Microsoft.Extensions.DependencyInjection;
using JasperFx.CodeGeneration.Util;

namespace Lamar.IoC.Frames
{
    public class InjectedServiceField : InjectedField, IServiceVariable
    {
        private bool _isOnlyOne;

        public InjectedServiceField(Instance instance) : base(instance.ServiceType,
            instance.DefaultArgName())
        {
            Instance = instance;
            _isOnlyOne = instance.IsOnlyOneOfServiceType;
        }

        public bool IsOnlyOne
        {
            private get => _isOnlyOne;
            set
            {
                _isOnlyOne = value;
                if (value)
                {
                    var defaultArgName = DefaultArgName(VariableType);
                    OverrideName("_" +defaultArgName);
                    CtorArg = defaultArgName;
                }
            }
        }

        public override string CtorArgDeclaration =>
            IsOnlyOne
                ? $"{ArgType.FullNameInCode()} {CtorArg}"
                : $"[Lamar.Named(\"{Instance.Name}\")] {ArgType.FullNameInCode()} {CtorArg}";

        public Instance Instance { get; }

        public override Expression ToVariableExpression(LambdaDefinition definition)
        {
            if (Instance.Lifetime == ServiceLifetime.Singleton)
            {
                var scope = definition.Context.As<Scope>();
                var @object = Instance.QuickResolve(scope);
                return Expression.Constant(@object);
            }
            
            // This needs to be inlined singletons
            throw new NotSupportedException();
        }
    }
}
