using System;
using JasperFx.Reflection;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;

namespace Lamar.Codegen
{
    #region sample_NoArgCreationFrame
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
    #endregion
    
    
    /*
     
        #region sample_NoArgCreationFrameCtor
        public NoArgCreationFrame(Type concreteType) 
        {
            // By creating the variable this way, we're
            // marking the variable as having been created
            // by this frame
            Output = new Variable(concreteType, this);
        }
        #endregion
     
     
        #region sample_NoArgCreationFrameCtor2
        public NoArgCreationFrame(Type concreteType) 
        {
            // By creating the variable this way, we're
            // marking the variable as having been created
            // by this frame
            Output = Create(concreteType);
        }
        #endregion
     */
}