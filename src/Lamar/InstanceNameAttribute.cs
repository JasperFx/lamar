using Lamar.IoC.Instances;

namespace Lamar
{
    /// <summary>
    /// Configures the Lamar instance name for resolving
    /// services by name
    /// </summary>
    public class InstanceNameAttribute : LamarAttribute
    {
        private readonly string _name;

        public InstanceNameAttribute(string name)
        {
            _name = name;
        }

        public override void Alter(Instance instance)
        {
            instance.Name = _name;
        }
    }
}