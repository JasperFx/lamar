using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Lamar.IoC.Diagnostics;

public class TextReportWriter
{
    private readonly int _columnCount;
    private readonly List<Line> _lines = new();

    public TextReportWriter(int columnCount)
    {
        _columnCount = columnCount;
    }

    public void AddDivider(char character)
    {
        _lines.Add(new DividerLine(character));
    }

    public void AddText(params string[] contents)
    {
        _lines.Add(new TextLine(contents));
    }

    public void AddContent(string contents)
    {
        _lines.Add(new PlainLine(contents));
    }

    public void Write(StringWriter writer)
    {
        var widths = CharacterWidth.For(_columnCount);

        foreach (var line in _lines) line.OverwriteCounts(widths);

        for (var i = 0; i < widths.Length - 1; i++)
        {
            var width = widths[i];
            width.Add(5);
        }

        foreach (var line in _lines)
        {
            writer.WriteLine();
            line.Write(writer, widths);
        }
    }

    public string Write()
    {
        using (var writer = new StringWriter())
        {
            Write(writer);

            return writer.ToString();
        }
    }

    public void DumpToDebug()
    {
        Debug.WriteLine(Write());
    }
}

internal class PlainLine : Line
{
    public PlainLine(string contents)
    {
        Contents = contents;
    }

    public string Contents { get; set; }

    #region Line Members

    public void OverwriteCounts(CharacterWidth[] widths)
    {
        // no-op
    }

    public void Write(TextWriter writer, CharacterWidth[] widths)
    {
        writer.WriteLine(Contents);
    }

    #endregion
}