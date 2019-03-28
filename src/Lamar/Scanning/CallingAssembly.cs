using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LamarCodeGeneration.Util;

namespace Lamar.Scanning
{
    public class CallingAssembly
    {
        internal static Assembly Find()
        {
            string trace = Environment.StackTrace;



            var parts = trace.Split('\n');

            for (int i = 4; i < parts.Length; i++)
            {
                var line = parts[i];
                var assembly = findAssembly(line);
                if (assembly != null && !isSystemAssembly(assembly))
                {
                    return assembly;
                }
            }

            return null;
        }

        private static bool isSystemAssembly(Assembly assembly)
        {
            if (assembly == null) return false;

            if (assembly.GetCustomAttributes<IgnoreAssemblyAttribute>().Any()) return true;

            return assembly.GetName().Name.StartsWith("System.");
        }

        private static readonly IList<string> _misses = new List<string>();

        private static Assembly findAssembly(string stacktraceLine)
        {
            var candidate = stacktraceLine.Trim().Substring(3);

            // Short circuit this
            if (candidate.StartsWith("System.")) return null;

            Assembly assembly = null;
            var names = candidate.Split('.');
            for (var i = names.Length - 2; i > 0; i--)
            {
                var possibility = String.Join(".", names.Take(i).ToArray());

                if (_misses.Contains(possibility)) continue;

                try
                {

                    assembly = Assembly.Load(new AssemblyName(possibility));
                    break;
                }
                catch
                {
                    _misses.Add(possibility);
                }
            }

            return assembly;
        }

        public static Assembly DetermineApplicationAssembly(object registry)
        {
            var assembly = registry.GetType().Assembly;
            return assembly.HasAttribute<IgnoreAssemblyAttribute>() ? CallingAssembly.Find() : assembly;
        }
    }
}
