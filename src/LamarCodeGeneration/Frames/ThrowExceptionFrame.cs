namespace LamarCodeGeneration.Frames
{
    public class ThrowExceptionFrame : SyncFrame
    {
        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }
}