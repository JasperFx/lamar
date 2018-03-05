using System.IO;

namespace Lamar.Util.TextWriting
{
    public interface Line
    {
        void WriteToConsole();
        void Write(TextWriter writer);
        int Width { get; }
    }
}