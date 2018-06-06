using System;
using Lamar.Codegen.Frames;
using Lamar.Compilation;

namespace Lamar.Codegen.Variables
{
    // SAMPLE: NowTimeVariableSource
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
    // ENDSAMPLE

    // SAMPLE: NowFetchFrame
    public class NowFetchFrame : SyncFrame
    {
        public NowFetchFrame(Type variableType)
        {
            Variable = new Variable(variableType, "now", this);
        }
        
        public Variable Variable { get; }
        
        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteLine($"var {Variable.Usage} = {Variable.VariableType.FullName}.{nameof(DateTime.UtcNow)};");
            Next?.GenerateCode(method, writer);
        }
    }
    // ENDSAMPLE

}
