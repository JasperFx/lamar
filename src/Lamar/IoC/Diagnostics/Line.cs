using System.IO;

namespace Lamar.IoC.Diagnostics
{
    internal interface Line
    {
        void OverwriteCounts(CharacterWidth[] widths);
        void Write(TextWriter writer, CharacterWidth[] widths);
    }
}