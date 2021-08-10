using System;
using System.Diagnostics;

namespace StructureMap.Docs.samples.quickstart
{
    internal class resolving_instances
    {
        private readonly IContainer container = new Container();

        public void get_instance_from_container_1()
        {
#region sample_quickstart-resolve-strongly-typed-instance
            var fooInstance = container.GetInstance<IFoo>();
#endregion
        }

        public void get_instance_from_container_2()
        {
#region sample_quickstart-resolve-weakly-typed-instance
            var foo = typeof (IFoo);

            var fooInstance = container.GetInstance(foo);
#endregion
        }

        public void resolve_all_instances_of_foo()
        {
#region sample_quickstart-resolve-all-instances-of-foo
            var fooInstances = container.GetAllInstances<IFoo>();
#endregion
        }

        public void resolve_unknown_instance_blah()
        {
#region sample_quickstart-resolve-unknown-instance-blah
            var blahInstance = container.TryGetInstance<IBlah>();

            Debug.Assert(blahInstance != null, String.Format("no default instance for {0}", typeof (IBlah).FullName));
#endregion
        }

        public void resolve_concrete_types()
        {
#region sample_quickstart-resolve-concrete-types
            var container = new Container();
            var weather1 = container.GetInstance<Weather>();

            var weather2 = container.GetInstance<Weather>();
            weather2 = container.GetInstance<Weather>(); //short version for above.
#endregion
        }
    }
}