using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JasperFx.Core;
using Lamar.IoC.Instances;
using LamarCodeGeneration.Model;
using Microsoft.Extensions.DependencyInjection;
using LamarCodeGeneration.Util;

namespace Lamar.IoC.Frames
{
    public class ResolverVariables : IEnumerable<Variable>
    {
        public int VariableSequence { get; set; }
        
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

        public void MakeNamesUnique()
        {
            var duplicateGroups = _all.GroupBy(x => x.Usage).Where(x => x.Count() > 1).ToArray();
            foreach (var @group in duplicateGroups)
            {
                var i = 0;
                foreach (var variable in group)
                {
                    variable.OverrideName(variable.Usage + ++i);
                }
            }
        }
    }
}
