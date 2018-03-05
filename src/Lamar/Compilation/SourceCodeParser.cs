using System.IO;
using System.Linq;
using Lamar.Util;

namespace Lamar.Compilation
{
    public class SourceCodeParser
    {
        private readonly LightweightCache<string, string> _code = new LightweightCache<string, string>(name => "UNKNOWN");

        private readonly StringWriter _current;
        private readonly string _name;

        public SourceCodeParser(string code)
        {
            foreach (var line in code.ReadLines())
            {
                if (_current == null)
                {
                    if (line.IsEmpty()) continue;

                    if (line.Trim().StartsWith("// START"))
                    {
                        _name = line.Split(':').Last().Trim();
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

        public string CodeFor(string typeName)
        {
            return _code[typeName];
        }
    }
}
