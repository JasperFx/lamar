using System;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using Lamar.Compilation;
using Lamar.Util;

namespace Lamar.Codegen
{
    public class NoArgCreationVariable : Variable
    {
        public NoArgCreationVariable(Type variableType) : base(variableType)
        {
            Creator = new NoArgCreationFrame(this);
        }
    }

    public class NoArgCreationFrame : Frame
    {
        private readonly Variable _output;

        public NoArgCreationFrame(Variable variable) : base(false)
        {
            _output = variable;

            creates.Fill(_output);
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            var creation = $"var {_output.Usage} = new {_output.VariableType.FullName.Replace("+", ".")}()";

            if (_output.VariableType.CanBeCastTo<IDisposable>())
            {
                writer.UsingBlock(creation, w => Next?.GenerateCode(method, w));
            }
            else
            {
                writer.WriteLine(creation + ";");
                Next?.GenerateCode(method, writer);
            }


        }
    }
}
