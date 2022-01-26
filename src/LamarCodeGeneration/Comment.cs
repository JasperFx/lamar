namespace LamarCodeGeneration
{
    public interface IComment
    {
        void Write(ISourceWriter writer);
    }

    public class OneLineComment : IComment
    {
        public string Text { get; }

        public OneLineComment(string text)
        {
            Text = text;
        }

        public void Write(ISourceWriter writer)
        {
            writer.WriteComment(Text);
        }
    }
    
    // TODO -- later, add Xml API comments and
    // multi-line comments
}