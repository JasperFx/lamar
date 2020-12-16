using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LamarCodeGeneration.Util;

namespace Lamar.IoC.Diagnostics
{
    public class ModelQuery
    {
        public string Namespace;
        public Type ServiceType;
        public Assembly Assembly;
        public string TypeName;

        public IEnumerable<IServiceFamilyConfiguration> Query(IModel model)
        {
            var enumerable = model.ServiceTypes;

            if (Namespace.IsNotEmpty())
            {
                enumerable = enumerable.Where(x => LamarCodeGeneration.Util.TypeExtensions.IsInNamespace(x.ServiceType, Namespace));
            }

            if (ServiceType != null)
            {
                enumerable = enumerable.Where(x => x.ServiceType == ServiceType);
            }

            if (Assembly != null)
            {
                enumerable = enumerable.Where(x => x.ServiceType.GetTypeInfo().Assembly == Assembly);
            }

            if (TypeName.IsNotEmpty())
            {
                enumerable = enumerable.Where(x => x.ServiceType.Name.ToLower().Contains(TypeName.ToLower()));
            }

            return enumerable;
        }
    }
}