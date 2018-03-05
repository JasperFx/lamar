using System.Collections.Generic;
using Lamar.Codegen;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Methods;
using Lamar.Codegen.Variables;
using Lamar.Compilation;

namespace Lamar.Testing.Codegen
{
    public class StubGeneratedMethod : IGeneratedMethod
    {
        public string MethodName { get; set; }

        public bool Virtual { get; set; }

        public AsyncMode AsyncMode { get; set; } = AsyncMode.ReturnFromLastNode;

        public IEnumerable<Argument> Arguments { get; set; } = new Argument[0];

        public InjectedField[] Fields { get; set; } = new InjectedField[0];

        public IList<Frame> Frames { get; } = new List<Frame>();

        public IList<IVariableSource> Sources { get; } = new List<IVariableSource>();

        public IList<Variable> DerivedVariables { get; } = new List<Variable>();

        public void WriteMethod(ISourceWriter writer)
        {
            
        }

        public void ArrangeFrames(GeneratedType type)
        {
            
        }
    }
}