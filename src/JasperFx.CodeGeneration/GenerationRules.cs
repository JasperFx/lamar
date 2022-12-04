using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JasperFx.CodeGeneration.Model;
using JasperFx.Core;

namespace JasperFx.CodeGeneration;

public enum TypeLoadMode
{
    /// <summary>
    ///     Always generate new types at runtime. This is appropriate for
    ///     development time when configuration may be in flux
    /// </summary>
    Dynamic,

    /// <summary>
    ///     Try to load generated types from the target application assembly,
    ///     but generate types if there is no pre-built types and export
    ///     the new source code
    /// </summary>
    Auto,

    /// <summary>
    ///     Types must be loaded from the pre-built application assembly, or
    ///     the loading will throw exceptions
    /// </summary>
    Static
}

public class GenerationRules
{
    public readonly IList<Assembly> Assemblies = new List<Assembly>();

    public readonly IDictionary<string, object> Properties = new Dictionary<string, object>();

    public readonly IList<IVariableSource> Sources = new List<IVariableSource>();

    public GenerationRules(string applicationNamespace) : this()
    {
        GeneratedNamespace = applicationNamespace;
    }

    public GenerationRules(string applicationNamespace, TypeLoadMode typeLoadMode) : this(applicationNamespace)
    {
        TypeLoadMode = typeLoadMode;
    }

    public GenerationRules()
    {
    }

    public bool SourceCodeWritingEnabled { get; set; } = true;


    public string GeneratedNamespace { get; set; } = "Internal.Generated";

    [Obsolete("Use GeneratedNamespace instead")]
    public string ApplicationNamespace
    {
        get => GeneratedNamespace;
        set => GeneratedNamespace = value;
    }

    public TypeLoadMode TypeLoadMode { get; set; } = TypeLoadMode.Dynamic;

    public string GeneratedCodeOutputPath { get; set; } = "Internal/Generated";

    public Assembly ApplicationAssembly { get; set; } = Assembly.GetEntryAssembly();

    /// <summary>
    ///     Reference the given assembly in the compilation
    /// </summary>
    /// <param name="assembly"></param>
    public void ReferenceAssembly(Assembly assembly)
    {
        Assemblies.Fill(assembly);
    }

    /// <summary>
    ///     Recursively reference assemblies from the supplied types, including generic
    ///     argument types
    /// </summary>
    /// <param name="types"></param>
    public void ReferenceTypes(params Type[] types)
    {
        foreach (var assembly in WalkReferencedAssemblies.ForTypes(types).Distinct()) Assemblies.Fill(assembly);
    }
}

/// <summary>
///     Find unique assemblies from the supplied types, including types
///     from generic arguments
/// </summary>
public static class WalkReferencedAssemblies
{
    public static IEnumerable<Assembly> ForTypes(params Type[] types)
    {
        var stack = new Stack<Type>();

        foreach (var type in types)
        {
            stack.Push(type);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                yield return current.Assembly;

                if (!current.IsGenericType || current.IsGenericTypeDefinition)
                {
                    continue;
                }

                var typeArguments = current.GetGenericArguments();
                foreach (var typeArgument in typeArguments) stack.Push(typeArgument);
            }
        }
    }
}