using System;
using System.IO;
using JasperFx.Core;

namespace LamarCodeGeneration;

public class SourceWriter : ISourceWriter, IDisposable
{
    private readonly StringWriter _writer = new();
    private string _leadingSpaces = "";

    private int _level;

    public void Dispose()
    {
        _writer?.Dispose();
    }

    public int IndentionLevel
    {
        get => _level;
        set
        {
            _level = value;
            _leadingSpaces = "".PadRight(_level * 4);
        }
    }

    public void BlankLine()
    {
        _writer.WriteLine();
    }

    public void Write(string text = null)
    {
        if (text.IsEmpty())
        {
            BlankLine();
            return;
        }

        text.ReadLines(line =>
        {
            line = line.Replace('`', '"');

            if (line.IsEmpty())
            {
                BlankLine();
            }
            else if (line.StartsWith("BLOCK:"))
            {
                WriteLine(line.Substring(6));
                StartBlock();
            }
            else if (line.StartsWith("END"))
            {
                FinishBlock(line.Substring(3));
            }
            else
            {
                WriteLine(line);
            }
        });
    }

    public void WriteLine(string text)
    {
        _writer.WriteLine(_leadingSpaces + text);
    }

    public void FinishBlock(string extra = null)
    {
        if (IndentionLevel == 0)
        {
            throw new InvalidOperationException("Not currently in a code block");
        }

        IndentionLevel--;

        if (extra.IsEmpty())
        {
            WriteLine("}");
        }
        else
        {
            WriteLine("}" + extra);
        }


        BlankLine();
    }

    private void StartBlock()
    {
        WriteLine("{");
        IndentionLevel++;
    }

    public string Code()
    {
        return _writer.ToString();
    }

    internal class BlockMarker : IDisposable
    {
        private readonly SourceWriter _parent;

        public BlockMarker(SourceWriter parent)
        {
            _parent = parent;
        }

        public void Dispose()
        {
            _parent.FinishBlock();
        }
    }
}