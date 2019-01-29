using System;
using System.Collections.Generic;
using LamarCompiler;
using LamarCompiler.Frames;
using LamarCompiler.Model;
using Newtonsoft.Json;

namespace LamarRest.Internal.Frames
{
    /// <summary>
    /// Serializes the incoming message
    /// </summary>
    public class SerializeJsonFrame : SyncFrame
    {
        private readonly Type _inputType;
        private Variable _input;

        public SerializeJsonFrame(Type inputType)
        {
            _inputType = inputType;
            
            Json = new Variable(typeof(string), "json", this);
        }
        
        public Variable Json { get; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            var methodName = $"{typeof(JsonConvert).FullNameInCode()}.{nameof(JsonConvert.SerializeObject)}";

            writer.Write($"var {Json.Usage} = {methodName}({_input.Usage});");
            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            _input = chain.FindVariable(_inputType);
            yield return _input;
        }
    }
}