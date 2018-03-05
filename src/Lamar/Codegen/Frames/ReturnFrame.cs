using Lamar.Compilation;

namespace Lamar.Codegen.Frames
{
    public class ReturnFrame : SyncFrame
    {
        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteReturnStatement(method);
        }
    }
}
