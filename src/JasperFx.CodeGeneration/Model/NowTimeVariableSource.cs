using System;
using JasperFx.CodeGeneration.Frames;

namespace JasperFx.CodeGeneration.Model;

#region sample_NowTimeVariableSource

public class NowTimeVariableSource : IVariableSource
{
    public bool Matches(Type type)
    {
        return type == typeof(DateTime) || type == typeof(DateTimeOffset);
    }

    public Variable Create(Type type)
    {
        if (type == typeof(DateTime))
        {
            return new NowFetchFrame(typeof(DateTime)).Variable;
        }

        if (type == typeof(DateTimeOffset))
        {
            return new NowFetchFrame(typeof(DateTimeOffset)).Variable;
        }

        throw new ArgumentOutOfRangeException(nameof(type), "Only DateTime and DateTimeOffset are supported");
    }
}

#endregion

#region sample_NowFetchFrame

public class NowFetchFrame : SyncFrame
{
    public NowFetchFrame(Type variableType)
    {
        // Notice how "this" frame is passed into the variable
        // class constructor as the creator
        Variable = new Variable(variableType, "now", this);
    }

    public Variable Variable { get; }

    public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
    {
        writer.WriteLine($"var {Variable.Usage} = {Variable.VariableType.FullName}.{nameof(DateTime.UtcNow)};");
        Next?.GenerateCode(method, writer);
    }
}

#endregion