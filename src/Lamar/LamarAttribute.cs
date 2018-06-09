using System;
using Lamar.IoC.Instances;

namespace Lamar
{
    // SAMPLE: LamarAttribute
    /// <summary>
    /// Base class for custom configuration attributes
    /// </summary>
    public abstract class LamarAttribute : Attribute
    {
        /// <summary>
        /// Make configuration alterations to a single IConfiguredInstance object
        /// </summary>
        /// <param name="instance"></param>    
        public virtual void Alter(IConfiguredInstance instance)
        {
        }

        /// <summary>
        /// Make configuration changes to the most generic form of Instance
        /// </summary>
        /// <param name="instance"></param>
        public virtual void Alter(Instance instance)
        {
            
        }
    }
    // ENDSAMPLE
}