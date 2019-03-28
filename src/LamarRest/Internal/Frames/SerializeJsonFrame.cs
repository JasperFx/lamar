using System;
using System.Collections.Generic;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using LamarCompiler;
using Newtonsoft.Json;

namespace LamarRest.Internal.Frames
{
    /// <summary>
    /// Generates code to serializes the incoming message and creates a variable
    /// in the method called "json"
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
            var serializeMethodName = $"{typeof(JsonConvert).FullNameInCode()}.{nameof(JsonConvert.SerializeObject)}";

            writer.BlankLine();
            writer.WriteComment($"From {nameof(SerializeJsonFrame)}");
            writer.Write($"var {Json.Usage} = {serializeMethodName}({_input.Usage});");
            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            _input = chain.FindVariable(_inputType);
            yield return _input;
        }
    }
}