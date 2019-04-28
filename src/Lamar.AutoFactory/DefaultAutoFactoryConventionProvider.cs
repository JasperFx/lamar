using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lamar.AutoFactory
{
    public class DefaultAutoFactoryConventionProvider : IAutoFactoryConventionProvider
    {
        public IAutoFactoryMethodDefinition GetMethodDefinition(MethodInfo methodInfo, IList<object> arguments)
        {
            if (methodInfo.Name.StartsWith("GetNames", StringComparison.OrdinalIgnoreCase)
                && methodInfo.IsGenericMethod
                && methodInfo.GetGenericArguments().Any()
                && methodInfo.ReturnType.IsAssignableFrom(typeof(List<string>)))
            {
                return new AutoFactoryMethodDefinition(AutoFactoryMethodType.GetNames, methodInfo.GetGenericArguments().First(), null);
            }

            var pluginType = methodInfo.ReturnType;

            // do nothing with void methods for now
            if (pluginType == typeof(void))
            {
                return null;
            }

            var name = tryGetInstanceName(methodInfo, arguments);

            var isNamed = !string.IsNullOrEmpty(name);

            return new AutoFactoryMethodDefinition(AutoFactoryMethodType.GetInstance, pluginType, name);
        }

        private static string tryGetInstanceName(MethodInfo methodInfo, IList<object> arguments)
        {
            return methodInfo.Name.StartsWith("GetNamed", StringComparison.OrdinalIgnoreCase)
                   && arguments.Any()
                ? Convert.ToString(arguments.First())
                : null;
        }
    }
}