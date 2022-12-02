using JasperFx.Core;

namespace LamarCodeGeneration.Model;

public class IfStyle
{
    public static readonly IfStyle If = new("if");
    public static readonly IfStyle ElseIf = new("else if");
    public static readonly IfStyle Else = new("else");
    public static readonly IfStyle None = new("else", false);
    private readonly bool _writes;

    private IfStyle(string code, bool writes = true)
    {
        _writes = writes;
        Code = code;
    }

    public string Code { get; }

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