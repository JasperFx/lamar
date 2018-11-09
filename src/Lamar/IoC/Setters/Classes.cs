using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lamar.IoC.Instances;
using Lamar.Util;

namespace Lamar.IoC.Setters
{

    public class Setter
    {
        public PropertyInfo Property { get; }
        public Instance Target { get; }

        public Setter(PropertyInfo property, Instance target)
        {
            Property = property;
            Target = target;
        }
        
        
    }
    

    public interface ISetterPolicy : ILamarPolicy
    {
        bool Matches(PropertyInfo prop);
    }

    public class SetterAttributePolicy : ISetterPolicy
    {
        public bool Matches(PropertyInfo prop)
        {
            return prop.HasAttribute<SetterPropertyAttribute>();
        }
    }
    

}