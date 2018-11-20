using System.IO;

namespace LamarCompiler.Util.TextWriting
{
    public interface IColumn
    {
        void WatchData(string contents);
        int Width { get; }
        void Write(TextWriter writer, string text);
        void WriteToConsole(string text);
    }
}