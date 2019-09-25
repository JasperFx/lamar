using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
#if !NET461
using System.Runtime.Loader;
#endif

namespace Lamar.Scanning.Conventions
{
	internal static class LamarAssemblyContext
	{
#if NET461
		public static readonly ILamarAssemblyLoadContext Loader = new CustomAssemblyLoadContext();
#else
		public static readonly ILamarAssemblyLoadContext Loader = new AssemblyLoadContextWrapper(System.Runtime.Loader.AssemblyLoadContext.Default);
#endif
	}

		public static class AssemblyFinder
    {
        public static IEnumerable<Assembly> FindAssemblies(Action<string> logFailure, bool includeExeFiles)
        {
            string path;
            try {
                path = AppContext.BaseDirectory;
            }
            catch (Exception) {
                path = System.IO.Directory.GetCurrentDirectory();
            }

            return FindAssemblies(path, logFailure, includeExeFiles);
        }

        public static IEnumerable<Assembly> FindAssemblies(string assemblyPath, Action<string> logFailure, bool includeExeFiles)
        {
            var dllFiles = Directory.EnumerateFiles(assemblyPath, "*.dll", SearchOption.AllDirectories);
            var files = dllFiles;

            if(includeExeFiles)
            {
                var exeFiles = Directory.EnumerateFiles(assemblyPath, "*.exe", SearchOption.AllDirectories);
                files = dllFiles.Concat(exeFiles);
            }

            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);
                Assembly assembly = null;

                try
                {
                    assembly = LamarAssemblyContext.Loader.LoadFromAssemblyName(new AssemblyName(name));
                }
                catch (Exception)
                {
                    try
                    {
                        assembly = LamarAssemblyContext.Loader.LoadFromAssemblyPath(file);
                    }
                    catch (Exception)
                    {
                        logFailure(file);
                    }
                }

                if (assembly != null)
                {
                    yield return assembly;
                }
            }
        }




        internal static IEnumerable<Assembly> FindAssemblies(Func<Assembly, bool> filter,
            Action<string> onDirectoryFound = null, bool includeExeFiles=false)
        {
            if (filter == null)
            {
                filter = a => true;
            }

            if (onDirectoryFound == null)
            {
                onDirectoryFound = dir => { };
            }

            return FindAssemblies(file => { }, includeExeFiles: includeExeFiles).Where(filter);
        }
    }
		
        internal interface ILamarAssemblyLoadContext
        {
            Assembly LoadFromStream(Stream assembly);
            Assembly LoadFromAssemblyName(AssemblyName assemblyName);
            Assembly LoadFromAssemblyPath(string assemblyName);
        }

#if !NET461
	public sealed class CustomAssemblyLoadContext : AssemblyLoadContext, ILamarAssemblyLoadContext
	{
		protected override Assembly Load(AssemblyName assemblyName)
		{
			return Assembly.Load(assemblyName);
		}

		Assembly ILamarAssemblyLoadContext.LoadFromAssemblyName(AssemblyName assemblyName)
		{
			return Load(assemblyName);
		}
	}

	public sealed class AssemblyLoadContextWrapper : ILamarAssemblyLoadContext
	{
		private readonly AssemblyLoadContext ctx;

		public AssemblyLoadContextWrapper(AssemblyLoadContext ctx)
		{
			this.ctx = ctx;
		}

		public Assembly LoadFromStream(Stream assembly)
		{
			return ctx.LoadFromStream(assembly);
		}

		public Assembly LoadFromAssemblyName(AssemblyName assemblyName)
		{
			return ctx.LoadFromAssemblyName(assemblyName);
		}

		public Assembly LoadFromAssemblyPath(string assemblyName)
		{
			return ctx.LoadFromAssemblyPath(assemblyName);
		}
	}
#else
        public class CustomAssemblyLoadContext : ILamarAssemblyLoadContext
        {
            public Assembly LoadFromStream(Stream assembly)
            {
                if (assembly is MemoryStream memStream)
                {
                    return Assembly.Load(memStream.ToArray());
                }

                using (var stream = new MemoryStream())
                {
                    assembly.CopyTo(stream);
                    return Assembly.Load(stream.ToArray());
                }
            }
		
            Assembly ILamarAssemblyLoadContext.LoadFromAssemblyName(AssemblyName assemblyName)
            {
                return Assembly.Load(assemblyName);
            }

            public Assembly LoadFromAssemblyPath(string assemblyName)
            {
                return Assembly.LoadFrom(assemblyName);
            }

            public Assembly LoadFromAssemblyName(string assemblyName)
            {
                return Assembly.Load(assemblyName);
            }
        }
#endif
}
