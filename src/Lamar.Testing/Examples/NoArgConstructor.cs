using System;
using Baseline;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;

namespace Lamar.Codegen
{
    // SAMPLE: NoArgCreationFrame
    public class NoArgCreationFrame : SyncFrame
    {
        public NoArgCreationFrame(Type concreteType) 
        {
            // By creating the variable this way, we're
            // marking the variable as having been created
            // by this frame
            Output = new Variable(concreteType, this);
        }

        public Variable Output { get; }

        // You have to override this method
        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            var creation = $"var {Output.Usage} = new {Output.VariableType.FullNameInCode()}()";

            if (Output.VariableType.CanBeCastTo<IDisposable>())
            {
                // there is an ISourceWriter shortcut for this, but this makes
                // a better code demo;)
                writer.Write($"BLOCK:using ({creation})");
                Next?.GenerateCode(method, writer);
                writer.FinishBlock();
            }
            else
            {
                writer.WriteLine(creation + ";");
                Next?.GenerateCode(method, writer);
            }
        }
    }
    // ENDSAMPLE
    
    
    /*
     
        // SAMPLE: NoArgCreationFrameCtor
        public NoArgCreationFrame(Type concreteType) 
        {
            // By creating the variable this way, we're
            // marking the variable as having been created
            // by this frame
            Output = new Variable(concreteType, this);
        }
        // ENDSAMPLE
     
     
        // SAMPLE: NoArgCreationFrameCtor2
        public NoArgCreationFrame(Type concreteType) 
        {
            // By creating the variable this way, we're
            // marking the variable as having been created
            // by this frame
            Output = Create(concreteType);
        }
        // ENDSAMPLE
     */
}