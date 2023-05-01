using System;
using System.Collections.Generic;
using System.Reflection;
using JasperFx.Core.Reflection;

namespace Lamar;

/// <summary>
///     Marks a method with no parameters as a method that validates an instance.  StructureMap
///     uses this method to validate the configuration file.  If the method does not throw an
///     exception, the object is assumed to be valid.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ValidationMethodAttribute : Attribute
{
    /// <summary>
    ///     Returns an array of any MethodInfo's on a Type that are marked as ValidationMethod
    /// </summary>
    /// <param name="objectType">CLR Type to search for validation methods</param>
    /// <returns></returns>
    public static MethodInfo[] GetValidationMethods(Type objectType)
    {
        var methodList = new List<MethodInfo>();

        var methods = objectType.GetMethods();
        foreach (var method in methods)
        {
            var att = method.GetAttribute<ValidationMethodAttribute>();


            if (att == null)
            {
                continue;
            }

            if (method.GetParameters().Length > 0)
            {
                var msg =
                    $"Method *{method.Name}* in Class *{objectType.AssemblyQualifiedName}* cannot be a validation method because it has parameters";
                throw new ArgumentException(msg);
            }

            methodList.Add(method);
        }

        var returnValue = new MethodInfo[methodList.Count];
        methodList.CopyTo(returnValue, 0);

        return returnValue;
    }
}