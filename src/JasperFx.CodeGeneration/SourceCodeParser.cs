using System;
using System.IO;
using System.Linq;
using JasperFx.CodeGeneration.Util;
using JasperFx.Core;

namespace JasperFx.CodeGeneration;

internal class SourceCodeParser : IDisposable
{
    private readonly LightweightCache<string, string> _code = new(name => "UNKNOWN");

    private readonly StringWriter _current;
    private readonly string _name;

    internal SourceCodeParser(string code)
    {
        foreach (var line in code.ReadLines())
        {
            if (_current == null)
            {
                if (line.IsEmpty())
                {
                    continue;
                }

                if (line.Trim().StartsWith("// START"))
                {
                    _name = line.Split(':').Last().Trim();

                    // dispose the old writer before overriding the reference
                    _current?.Dispose();

                    _current = new StringWriter();
                }
            }
            else
            {
                if (line.Trim().StartsWith("// END"))
                {
                    var classCode = _current.ToString();
                    _code[_name] = classCode;

                    _current = null;
                    _name = null;
                }
                else
                {
                    _current.WriteLine(line);
                }
            }
        }
    }

    public void Dispose()
    {
        _current?.Dispose();
    }

    public string CodeFor(string typeName)
    {
        return _code[typeName];
    }
}