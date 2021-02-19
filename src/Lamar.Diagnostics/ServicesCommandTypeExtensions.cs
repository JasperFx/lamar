using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Baseline;
using LamarCodeGeneration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lamar.Diagnostics
{
    public static class ServicesCommandTypeExtensions
    {
        
        public static bool IsEnumerable(this Type type, out Type elementType)
        {
            if (type.Closes(typeof(IEnumerable<>)) && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                elementType = type.GetGenericArguments()[0];
                return true;
            }

            elementType = null;
            return false;
        }
        
        public static bool IsLogger(this Type type, out Type innerType)
        {
            if (type.Closes(typeof(ILogger<>)) && type.GetGenericTypeDefinition() == typeof(ILogger<>))
            {
                innerType = type.GetGenericArguments()[0];
                return true;
            }

            innerType = null;
            return false;
        }

        public static bool IsOption(this Type type, out Type optionType)
        {
            if (type.Closes(typeof(IOptions<>)) && type != typeof(IOptions<>))
            {
                optionType = type.GetGenericArguments().First();
                return true;
            }

            optionType = null;
            return false;

        }


        
        public static Type ResolveServiceType(this Type type)
        {
            if (IsEnumerable(type, out var elementType))
            {
                return ResolveServiceType(elementType);
            }

            if (IsOption(type, out var optionType))
            {
                return ResolveServiceType(optionType);
            }

            if (IsLogger(type, out var loggedType))
            {
                return ResolveServiceType(loggedType);
            }

            return type;
        }
        
        public static Assembly AssemblyForType(this Type type)
        {
            if (IsEnumerable(type, out var elementType))
            {
                return AssemblyForType(elementType);
            }

            if (IsOption(type, out var optionType))
            {
                return AssemblyForType(optionType);
            }

            if (IsLogger(type, out var loggedType))
            {
                return AssemblyForType(loggedType);
            }

            return type.Assembly;
        }


        
        
        public static string CleanFullName(this Type type)
        {
            try
            {
                if (type.IsOpenGeneric())
                {
                    var parts = type.FullNameInCode().Split('`');
                    var argCount = int.Parse(parts[1]) - 1;

                    return $"{parts[0]}<{"".PadLeft(argCount, ',')}>";
                }
                else if (type.IsEnumerable(out var elementType))
                {
                    return $"IEnumerable<{elementType.FullNameInCode()}>";
                }
                else if (type.IsOption(out var optionType))
                {
                    return $"IOptions<{optionType.FullNameInCode()}>";
                }
                else if (type.IsLogger(out var loggedType))
                {
                    return $"ILogger<{loggedType.FullNameInCode()}>";
                }
                else
                {
                    return type.FullNameInCode();
                }
            }
            catch (Exception)
            {
                return type?.FullName;
            }
        }
        
        public static string BoldText(this object data)
        {
            return $"[bold]{data}[/]";
        }


    }
}