using System;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Frames;
using JasperFx.CodeGeneration.Model;
using JasperFx.Core.Reflection;

namespace Lamar.IoC.Frames;

public class GetInstanceFromNestedContainerFrame : SyncFrame
{
    private readonly Variable _nested;


    public GetInstanceFromNestedContainerFrame(Variable nested, Type serviceType)
    {
        _nested = nested;
        uses.Add(_nested);

        Variable = new Variable(serviceType, this);
    }

    /// <summary>
    ///     <summary>
    ///         Optional code fragment to write at the beginning of this
    ///         type in code
    ///     </summary>
    public ICodeFragment? Header { get; set; }

    public Variable Variable { get; }


    /// <summary>
    ///     Add a single line comment as the header to this type
    /// </summary>
    /// <param name="text"></param>
    public void Comment(string text)
    {
        Header = new OneLineComment(text);
    }

    /// <summary>
    ///     Add a multi line comment as the header to this type
    /// </summary>
    /// <param name="text"></param>
    public void MultiLineComment(string text)
    {
        Header = new MultiLineComment(text);
    }

    public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
    {
        if (Header != null)
        {
            writer.WriteLine("");
            Header.Write(writer);
        }

        writer.Write(
            $"var {Variable.Usage} = {_nested.Usage}.{nameof(IContainer.GetInstance)}<{Variable.VariableType.FullNameInCode()}>();");
        Next?.GenerateCode(method, writer);
    }
}