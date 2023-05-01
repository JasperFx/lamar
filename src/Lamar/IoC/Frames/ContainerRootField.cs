using System.Collections.Generic;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Frames;
using JasperFx.CodeGeneration.Model;
using JasperFx.Core.Reflection;

namespace Lamar.IoC.Frames;

public class NestedContainerCreation : AsyncFrame
{
    public NestedContainerCreation()
    {
        Root = new InjectedField(typeof(IContainer), "rootContainer");
        Nested = new Variable(typeof(IContainer), "nestedContainer", this);
    }

    public Variable Root { get; }

    public Variable Nested { get; }

    public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
    {
        yield return Root;
    }

    public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
    {
        writer.Write(
            $"await using var {Nested.Usage} = ({typeof(IContainer).FullNameInCode()})_rootContainer.{nameof(IContainer.GetNestedContainer)}();");
        Next?.GenerateCode(method, writer);
    }
}