namespace Lamar.Compilation
{
    public interface ISourceWriter
    {
        void BlankLine();
        void Write(string text = null);
        void FinishBlock(string extra = null);
        void WriteLine(string text);
    }
}
