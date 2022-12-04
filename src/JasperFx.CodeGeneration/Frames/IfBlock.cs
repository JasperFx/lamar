using JasperFx.CodeGeneration.Model;

namespace JasperFx.CodeGeneration.Frames;

public class IfBlock : CompositeFrame
{
    public IfBlock(string condition, params Frame[] inner) : base(inner)
    {
        Condition = condition;
    }

    public IfBlock(Variable variable, params Frame[] inner) : this(variable.Usage, inner)
    {
    }

    public string Condition { get; }

    protected override void generateCode(GeneratedMethod method, ISourceWriter writer, Frame inner)
    {
        writer.Write($"BLOCK:if ({Condition})");
        inner.GenerateCode(method, writer);
        writer.FinishBlock();
    }
}