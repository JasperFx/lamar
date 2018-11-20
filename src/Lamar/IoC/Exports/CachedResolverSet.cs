using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lamar.IoC.Instances;
using Lamar.Scanning.Conventions;
using LamarCompiler.Util;

namespace Lamar.IoC.Exports
{
    public abstract class CachedResolverSet : ILamarPolicy
    {
        public readonly string ResolverLoaderClassName = "ResolverLoader";

        public CachedResolverSet()
        {
            Namespace = GetType().Assembly.GetName().Name + ".Internal.Resolvers";
        }

        /// <summary>
        ///     The namespace for the generated resolvers. The pathing of
        ///     the generated files will respect this namespace
        /// </summary>
        public string Namespace { get; set; }


        public virtual bool Include(GeneratedInstance instance)
        {
            return instance.ServiceType.Assembly == GetType().Assembly;
        }

        public virtual bool Exclude(GeneratedInstance instance)
        {
            return false;
        }

        private bool canBePrebuilt(GeneratedInstance instance)
        {
            if (instance.ImplementationType.MustBeBuiltWithFunc()) return false;

            // TODO -- try to address this later
            if (instance is ConstructorInstance i)
            {
                if (i.InlineDependencies.Any()) return false;
            }

            return true;
        }

        internal void Export(ServiceGraph serviceGraph, GeneratedInstance[] instances, string path)
        {
            var system = new FileSystem();

            if (system.DirectoryExists(path))
            {
                system.CleanDirectory(path);
            }
            else
            {
                system.CreateDirectory(path);
            }


            var matching = instances.Where(x => canBePrebuilt(x) && Include(x) && !Exclude(x)).ToArray();

            var typenames = new Dictionary<string, string>();

            foreach (var instance in matching) writeResolverCodeFile(serviceGraph, path, instance, system, typenames);

            writeResolverLoaderClass(serviceGraph, path, typenames, system);
        }

        private void writeResolverCodeFile(ServiceGraph serviceGraph, string path, GeneratedInstance instance,
            FileSystem system, Dictionary<string, string> typenames)
        {
            var assembly = serviceGraph.ToGeneratedAssembly(Namespace);
            var (className, code) = instance.GenerateResolverClassCode(assembly);

            system.WriteStringToFile(Path.Combine(path, className + ".cs"), code);

            typenames.Add(className, Namespace + "." + className);
        }

        private void writeResolverLoaderClass(ServiceGraph serviceGraph, string path, Dictionary<string, string> typenames,
            FileSystem system)
        {
            var resolverAssembly = serviceGraph.ToGeneratedAssembly(Namespace);
            var resolverType = resolverAssembly.AddType(ResolverLoaderClassName, typeof(IResolverLoader));
            var method = resolverType.MethodFor(nameof(IResolverLoader.ResolverTypes));
            method.Frames.Add(new ResolverDictFrame(typenames));

            var resolverCode = resolverAssembly.GenerateCode();

            system.WriteStringToFile(Path.Combine(path, ResolverLoaderClassName + ".cs"), resolverCode);
        }

        public Dictionary<string, Type> LoadResolvers()
        {
            var typeName = Namespace + "." + ResolverLoaderClassName;
            var type = GetType().Assembly.GetType(typeName);

            if (type == null) return null;
            
            var loader = Activator.CreateInstance(type).As<IResolverLoader>();
            return loader.ResolverTypes();
        }

        public bool TryLoadResolvers(out Dictionary<string, Type> dictionary)
        {
            var typeName = Namespace + "." + ResolverLoaderClassName;
            var type = GetType().Assembly.GetType(typeName);

            if (type == null)
            {
                dictionary = null;
                return false;
            }
            
            var loader = Activator.CreateInstance(type).As<IResolverLoader>();
            dictionary = loader.ResolverTypes();

            return true;
        }
    }
}