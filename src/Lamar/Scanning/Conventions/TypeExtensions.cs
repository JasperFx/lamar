using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LamarCodeGeneration.Util;

namespace Lamar.Scanning.Conventions
{
    public static class TypeExtensions
    {

        public static bool CanBeCreated(this Type type)
        {
            return type.IsConcrete() && type.GetConstructors().Any();
        }


        public static Type FindFirstInterfaceThatCloses(this Type TPluggedType, Type templateType)
        {
            return TPluggedType.FindInterfacesThatClose(templateType).FirstOrDefault();
        }

        public static IEnumerable<Type> FindInterfacesThatClose(this Type TPluggedType, Type templateType)
        {
            return rawFindInterfacesThatCloses(TPluggedType, templateType).Distinct();
        }

        private static IEnumerable<Type> rawFindInterfacesThatCloses(Type TPluggedType, Type templateType)
        {
            if (!TPluggedType.IsConcrete()) yield break;

            if (templateType.GetTypeInfo().IsInterface)
            {
                foreach (
                    var interfaceType in
                    TPluggedType.GetInterfaces()
                        .Where(type => type.GetTypeInfo().IsGenericType && (type.GetGenericTypeDefinition() == templateType)))
                {
                    yield return interfaceType;
                }
            }
            else if (TPluggedType.GetTypeInfo().BaseType.GetTypeInfo().IsGenericType &&
                     (TPluggedType.GetTypeInfo().BaseType.GetGenericTypeDefinition() == templateType))
            {
                yield return TPluggedType.GetTypeInfo().BaseType;
            }

            if (TPluggedType.GetTypeInfo().BaseType == typeof(object)) yield break;

            foreach (var interfaceType in rawFindInterfacesThatCloses(TPluggedType.GetTypeInfo().BaseType, templateType))
            {
                yield return interfaceType;
            }
        }
        
        public static bool CouldCloseTo(this Type openConcretion, Type closedInterface)
        {
            var openInterface = closedInterface.GetGenericTypeDefinition();
            var arguments = closedInterface.GetGenericArguments();

            var concreteArguments = openConcretion.GetGenericArguments();
            return arguments.Length == concreteArguments.Length && openConcretion.CanBeCastTo(openInterface);
        }
    }
}
