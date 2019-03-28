using System.Net.Http;
using LamarCodeGeneration;
using LamarCodeGeneration.Model;
using LamarCompiler;

namespace LamarRest
{
    public class HttpMethodVariable : Variable
    {
        public HttpMethodVariable(string httpMethod) : base(typeof(HttpMethod), $"new {typeof(HttpMethod).FullNameInCode()}(\"{httpMethod}\")")
        {
        }
    }
}