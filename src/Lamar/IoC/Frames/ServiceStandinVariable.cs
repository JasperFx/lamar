using System;
using Lamar.IoC.Instances;
using JasperFx.CodeGeneration.Model;

namespace Lamar.IoC.Frames
{
    public class ServiceStandinVariable : Variable
    {
        private Variable _inner;
        public Instance Instance { get; }

        public ServiceStandinVariable(Instance instance) : base(instance.ServiceType)
        {
            Instance = instance;
        }

        public void UseInner(Variable variable)
        {
            _inner = variable ?? throw new ArgumentNullException(nameof(variable));
            Dependencies.Add(variable);
        }

        public override string Usage
        {
            get => _inner?.Usage;
            protected set {
            {
                base.Usage = value;
            }}
        }

        public override int GetHashCode()
        {
            return _inner == null ? Instance.GetHashCode() : _inner.GetHashCode();
        }
    }
}