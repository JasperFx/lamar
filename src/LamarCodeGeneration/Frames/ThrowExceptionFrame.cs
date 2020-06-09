using System;
using System.Linq;
using LamarCodeGeneration.Util;

namespace LamarCodeGeneration.Frames
{
    public class ThrowExceptionFrame<T> : CodeFrame where T : Exception
    {
        public static string ToFormat(object[] values)
        {
            var index = 0;
            var parameters = values.Select(x => "{" + index++ + "}").Join(", ");

            return $"throw new {typeof(T).FullNameInCode()}({parameters});";
        }
        
        public ThrowExceptionFrame(params object[] values) : base(false, ToFormat(values), values)
        {
        }
    }

    public class ThrowExceptionFrame : SyncFrame
    {
        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }
}