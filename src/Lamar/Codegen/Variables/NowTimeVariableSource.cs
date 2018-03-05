using System;
using Lamar.Codegen.Frames;
using Lamar.Compilation;

namespace Lamar.Codegen.Variables
{
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
                return new NowVariable();
            }

            if (type == typeof(DateTimeOffset))
            {
                return new NowOffsetVariable();
            }

            throw new ArgumentOutOfRangeException(nameof(type), "Only DateTime and DateTimeOffset are supported");
        }
    }

    public class NowVariable : Variable
    {
        public static readonly string Now = "now";

        public NowVariable() : base(typeof(DateTime), Now)
        {
            Creator = new NowFetchFrame();
        }
    }

    public class NowFetchFrame : Frame
    {
        public NowFetchFrame() : base(false)
        {
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteLine($"var {NowVariable.Now} = {typeof(DateTime).FullName}.{nameof(DateTime.UtcNow)};");
            Next?.GenerateCode(method, writer);
        }
    }

    public class NowOffsetVariable : Variable
    {
        public static readonly string Now = "now";

        public NowOffsetVariable() : base(typeof(DateTimeOffset), Now)
        {
            Creator = new NowOffsetFetchFrame();
        }
    }

    public class NowOffsetFetchFrame : Frame
    {
        public NowOffsetFetchFrame() : base(false)
        {
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteLine($"var {NowVariable.Now} = {typeof(DateTimeOffset).FullName}.{nameof(DateTimeOffset.UtcNow)};");
            Next?.GenerateCode(method, writer);
        }
    }
}
