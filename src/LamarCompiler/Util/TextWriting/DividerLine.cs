using System;
using System.IO;

namespace LamarCompiler.Util.TextWriting
{
    public class DividerLine : Line
    {
        private readonly char _character;

        public DividerLine(char character)
        {
            _character = character;
        }

        public void WriteToConsole()
        {
            Write(Console.Out);
        }

        public void Write(TextWriter writer)
        {
            writer.WriteLine(string.Empty.PadRight(Width, _character));
        }

        public int Width
        {
            get; set;
        }
    }
}