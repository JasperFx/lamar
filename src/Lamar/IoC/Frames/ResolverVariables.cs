using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JasperFx.CodeGeneration.Model;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.IoC.Frames;

public class ResolverVariables : IEnumerable<Variable>, IMethodVariables
{
    private readonly List<Variable> _all = new();
    private readonly Dictionary<Instance, Variable> _tracking = new();

    public ResolverVariables()
    {
        Method = this;
    }

    public ResolverVariables(IMethodVariables method, IList<InjectedServiceField> fields)
    {
        Method = method;
        _all.AddRange(fields);

        foreach (var field in fields) _tracking[field.Instance] = field;
    }

    public int VariableSequence { get; set; }

    public IMethodVariables Method { get; }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<Variable> GetEnumerator()
    {
        return _all.GetEnumerator();
    }

    Variable IMethodVariables.FindVariable(Type type)
    {
        return null;
    }

    Variable IMethodVariables.FindVariableByName(Type dependency, string name)
    {
        return null;
    }

    bool IMethodVariables.TryFindVariableByName(Type dependency, string name, out Variable variable)
    {
        variable = default;
        return false;
    }

    Variable IMethodVariables.TryFindVariable(Type type, VariableSource source)
    {
        return null;
    }

    public Variable Resolve(Instance instance, BuildMode mode)
    {
        if (_tracking.TryGetValue(instance, out var variable))
        {
            return variable;
        }

        var fromOutside = Method.TryFindVariable(instance.ServiceType, VariableSource.NotServices);
        if (fromOutside != null && !(fromOutside is ServiceStandinVariable))
        {
            _all.Add(fromOutside);
            _tracking[instance] = fromOutside;

            return fromOutside;
        }

        variable = instance.CreateVariable(mode, this, false);
        _all.Add(variable);

        // Don't track it for possible reuse if it's transient
        if (instance.Lifetime == ServiceLifetime.Scoped)
        {
            _tracking[instance] = variable;
        }

        return variable;
    }

    public void MakeNamesUnique()
    {
        var duplicateGroups = _all.GroupBy(x => x.Usage).Where(x => x.Count() > 1).ToArray();
        foreach (var group in duplicateGroups)
        {
            var i = 0;
            foreach (var variable in group) variable.OverrideName(variable.Usage + ++i);
        }
    }
}