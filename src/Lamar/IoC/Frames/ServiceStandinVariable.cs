using System;
using JasperFx.CodeGeneration.Model;
using Lamar.IoC.Instances;

namespace Lamar.IoC.Frames;

public class ServiceStandinVariable : Variable
{
    private Variable _inner;

    public ServiceStandinVariable(Instance instance) : base(instance.ServiceType)
    {
        Instance = instance;
    }

    public Instance Instance { get; }

    public override string Usage
    {
        get => _inner?.Usage;
        protected set
        {
            {
                base.Usage = value;
            }
        }
    }

    public void UseInner(Variable variable)
    {
        _inner = variable ?? throw new ArgumentNullException(nameof(variable));
        Dependencies.Add(variable);
    }

    public override void OverrideName(string variableName)
    {
        _inner.OverrideName(variableName);
    }

    public override int GetHashCode()
    {
        return _inner == null ? Instance.GetHashCode() : _inner.GetHashCode();
    }
}