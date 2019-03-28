using System;
using System.Collections.Generic;
using System.Net.Http;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using LamarCompiler;
using Newtonsoft.Json;

namespace LamarRest.Internal.Frames
{
    public class DeserializeObjectFrame : AsyncFrame
    {
        private Variable _response;

        public DeserializeObjectFrame(Type returnType)
        {
            ReturnValue = new Variable(returnType, this);
        }
        
        public Variable ReturnValue { get; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            var responseContent = $"await {_response.Usage}.Content.ReadAsStringAsync()";
            var methodName = $"{typeof(JsonConvert).FullNameInCode()}.{nameof(JsonConvert.DeserializeObject)}";
            
            writer.BlankLine();
            writer.WriteComment($"From {nameof(DeserializeObjectFrame)}");
            writer.Write($"var {ReturnValue.Usage} = {methodName}<{ReturnValue.VariableType.FullNameInCode()}>({responseContent});");
            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            _response = chain.FindVariable(typeof(HttpResponseMessage));
            yield return _response;
        }
    }
}