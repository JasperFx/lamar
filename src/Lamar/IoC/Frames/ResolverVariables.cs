using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lamar.Codegen.Variables;
using Lamar.IoC.Instances;
using Lamar.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.IoC.Frames
{
    public class ResolverVariables : IEnumerable<Variable>
    {
        private readonly IList<Variable> _cached = new List<Variable>();
        private readonly IList<Variable> _all = new List<Variable>();

        public ResolverVariables()
        {
        }

        public ResolverVariables(IList<InjectedServiceField> fields)
        {
            _all.AddRange(fields);
            _cached.AddRange(fields);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Variable> GetEnumerator()
        {
            return _all.GetEnumerator();
        }

        public Variable[] AllFor(Instance instance)
        {
            return _cached.Where(x => x.RefersTo(instance)).ToArray();
        }

        public Variable Resolve(Instance instance, BuildMode mode)
        {
            if (instance.Lifetime == ServiceLifetime.Transient)
            {
                var transient = instance.CreateVariable(mode, this, false);


                var sameNamed = _all.Where(x => x.Usage == transient.Usage).ToArray();
                if (sameNamed.Length == 1)
                {
                    sameNamed.Single().OverrideName(transient.Usage + "1");
                    transient.OverrideName(transient.Usage + "2");
                }
                else if (sameNamed.Length > 1)
                {
                    transient.OverrideName(transient.Usage + (sameNamed.Length + 1));
                }

                _all.Add(transient);




                return transient;
            }

            var variable = AllFor(instance).SingleOrDefault();
            if (variable == null)
            {
                variable = instance.CreateVariable(mode, this, false);
                _all.Add(variable);
                _cached.Add(variable);
            }

            return variable;
        }

        public void Add(ServiceVariable variable)
        {
            // TODO -- have to do more on naming too
            var index = AllFor(variable.Instance).Length + 1;
            if (index > 1)
            {
                variable.OverrideName(variable.Usage + "_" + index);
            }

            _cached.Add(variable);
        }
    }
}
