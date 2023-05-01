using System;
using System.Collections.Generic;
using System.Linq;
using JasperFx.CodeGeneration.Frames;
using JasperFx.CodeGeneration.Model;
using JasperFx.Core.Reflection;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.IoC.Enumerables;

/// <summary>
///     Instance type to support .Net enumerable types that are not arrays
/// </summary>
/// <typeparam name="T"></typeparam>
public class ListInstance<T> : GeneratedInstance, IEnumerableInstance
{
    public ListInstance(Type serviceType) : base(serviceType, typeof(List<T>), ServiceLifetime.Transient)
    {
        Name = Variable.DefaultArgName(typeof(List<T>));
    }

    public IList<Instance> InlineDependencies { get; } = new List<Instance>();

    public Instance[] Elements { get; private set; }

    public override Frame CreateBuildFrame()
    {
        var variables = new ResolverVariables();
        var elements = Elements.Select(x => variables.Resolve(x, BuildMode.Dependency)).ToArray();
        variables.MakeNamesUnique();

        return new ListAssignmentFrame<T>(this, elements)
        {
            ReturnCreated = true
        };
    }

    protected override Variable generateVariableForBuilding(ResolverVariables variables, BuildMode mode, bool isRoot)
    {
        // This is goofy, but if the current service is the top level root of the resolver
        // being created here, make the dependencies all be Dependency mode
        var dependencyMode = isRoot && mode == BuildMode.Build ? BuildMode.Dependency : mode;

        var elements = Elements.Select(x => variables.Resolve(x, dependencyMode)).ToArray();

        return new ListAssignmentFrame<T>(this, elements).Variable;
    }

    protected override IEnumerable<Instance> createPlan(ServiceGraph services)
    {
        if (InlineDependencies.Any())
        {
            Elements = InlineDependencies.ToArray();
        }
        else
        {
            Elements = services.FindAll(typeof(T));
        }

        return Elements;
    }

    public override object QuickResolve(Scope scope)
    {
        return Elements.Select(x => x.QuickResolve(scope).As<T>()).ToList();
    }

    /// <summary>
    ///     Adds an inline dependency
    /// </summary>
    /// <param name="instance"></param>
    public void AddInline(Instance instance)
    {
        instance.Parent = this;
        InlineDependencies.Add(instance);
    }

    public override string ToString()
    {
        return $"List of all {typeof(T).NameInCode()}";
    }
}