
using System.Linq;
using JasperFx.CodeGeneration.Util;
using JasperFx.Core;

namespace JasperFx.RuntimeCompiler.Scenarios
{
    public class CodegenResult<TObject>
    {
        public CodegenResult(TObject o, string code)
        {
            Object = o;
            Code = code;
            LinesOfCode = code.ReadLines().Select(x => x.Trim()).ToArray();
        }

        public TObject Object { get; }
        public string Code { get; }
        public string[] LinesOfCode { get; }
    }
}