namespace LamarCodeGeneration;

public interface ICodeFragment
{
    void Write(ISourceWriter writer);
}

public class OneLineComment : ICodeFragment
{
    public OneLineComment(string text)
    {
        Text = text;
    }

    public string Text { get; }

    public void Write(ISourceWriter writer)
    {
        writer.WriteComment(Text);
    }
}

public static class ConditionalCompilation
{
    public static OneLineCodeFragment If(string target)
    {
        return new OneLineCodeFragment($"#if {target}");
    }

    public static OneLineCodeFragment EndIf()
    {
        return new OneLineCodeFragment("#endif");
    }
}

public class OneLineCodeFragment : ICodeFragment
{
    public OneLineCodeFragment(string text)
    {
        Text = text;
    }

    public string Text { get; }

    public void Write(ISourceWriter writer)
    {
        writer.WriteLine(Text);
    }
}

// TODO -- later, add Xml API comments and
// multi-line comments