using Lamar.Codegen;
using Lamar.Codegen.Variables;
using Lamar.IoC.Instances;
using Lamar.Scanning.Conventions;

namespace Lamar.IoC.Frames
{
    public class InjectedServiceField : InjectedField, IServiceVariable
    {
        private bool _isOnlyOne;

        public InjectedServiceField(Instance instance) : base(instance.ServiceType.MustBeBuiltWithFunc() ? typeof(object) : instance.ServiceType,
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

        public override string CtorArgDeclaration
        {
            get
            {
                if (Instance.ServiceType.MustBeBuiltWithFunc())
                {
                    return $"[Lamar.Named(\"{Instance.Name}\", \"{Instance.ServiceType.FullNameInCode()}\")] object {CtorArg}";
                }

                return IsOnlyOne
                    ? $"{ArgType.FullNameInCode()} {CtorArg}"
                    : $"[Lamar.Named(\"{Instance.Name}\")] {ArgType.FullNameInCode()} {CtorArg}";
            }
        }

        public Instance Instance { get; }
    }
}
