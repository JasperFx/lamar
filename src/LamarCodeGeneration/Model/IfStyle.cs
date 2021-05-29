using LamarCodeGeneration.Util;

namespace LamarCodeGeneration.Model
{
    public class IfStyle
    {
        private readonly bool _writes;
        public static readonly IfStyle If = new IfStyle("if");
        public static readonly IfStyle ElseIf = new IfStyle("else if");
        public static readonly IfStyle Else = new IfStyle("else");
        public static readonly IfStyle None = new IfStyle("else", false);

        public string Code { get; }

        private IfStyle(string code, bool writes = true)
        {
            _writes = writes;
            Code = code;
        }

        public void Open(ISourceWriter writer, string condition)
        {
            if (_writes)
            {
                writer.Write(condition.IsEmpty()
                    ? $"BLOCK:{Code}"
                    : $"BLOCK:{Code} ({condition})");
            }
        }

        public void Close(ISourceWriter writer)
        {
            if (_writes)
            {
                writer.FinishBlock();
            }
        }

        public override string ToString()
        {
            return Code;
        }
    }
}