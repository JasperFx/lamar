using System;
using JasperFx.Core.Reflection;
using Lamar.IoC.Instances;
using LamarCodeGeneration.Util;

namespace Lamar
{
    #region sample_IInstancePolicy
    /// <summary>
    /// Custom policy on Instance construction that is evaluated
    /// as part of creating a "build plan"
    /// </summary>
    
    public interface IInstancePolicy : ILamarPolicy
    {
        /// <summary>
        /// Apply any conventional changes to the configuration
        /// of a single Instance
        /// </summary>
        /// <param name="instance"></param>
        void Apply(Instance instance);
    }
    #endregion

    #region sample_ConfiguredInstancePolicy
    /// <summary>
    /// Base class for using policies against IConfiguredInstance registrations
    /// </summary>
    public abstract class ConfiguredInstancePolicy : IInstancePolicy
    {
        public void Apply(Instance instance)
        {
            if (instance is IConfiguredInstance)
            {
                apply(instance.As<IConfiguredInstance>());
            }
        }

        protected abstract void apply(IConfiguredInstance instance);
    }
    #endregion
}