using System.IO;

namespace LamarCodeGeneration.Util.TextWriting
{
    public interface Line
    {
        void WriteToConsole();
        void Write(TextWriter writer);
        int Width { get; }
    }
}