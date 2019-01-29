using System.Net.Http;
using LamarCompiler;
using LamarCompiler.Model;

namespace LamarRest.Internal.Frames
{
    public class JsonContentVariable : Variable
    {
        public JsonContentVariable(Variable json) : base(typeof(StringContent), $"new {typeof(StringContent).FullNameInCode()}({json.Usage}, System.Text.Encoding.UTF8, \"application/json\")")
        {
        }

    }
}