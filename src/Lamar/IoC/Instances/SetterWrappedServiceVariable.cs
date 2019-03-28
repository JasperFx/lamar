using Lamar.IoC.Frames;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;

namespace Lamar.IoC.Instances
{
    public class SetterWrappedServiceVariable : ServiceVariable
    {
        private readonly Setter _setter;

        public SetterWrappedServiceVariable(Setter setter, Instance instance, Frame creator, ServiceDeclaration declaration = ServiceDeclaration.ImplementationType) : base(instance, creator, declaration)
        {
            _setter = setter;
        }

        public override void OverrideName(string variableName)
        {
            _setter.OverrideName(variableName);
            base.OverrideName(variableName);
        }
    }
}