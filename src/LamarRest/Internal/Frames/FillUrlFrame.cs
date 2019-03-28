using System.Reflection;
using Baseline.Reflection;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;

namespace LamarRest.Internal.Frames
{
    public class FillUrlFrame : SyncFrame
    {
        public FillUrlFrame(MethodInfo method)
        {
            Url = new Variable(typeof(string), "url");
            Pattern = method.GetAttribute<PathAttribute>().Path;
        }

        public Variable Url { get; }
        
        
        public string Pattern { get; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.BlankLine();
            writer.WriteComment($"From {nameof(FillUrlFrame)}");
            writer.Write($"var {Url.Usage} = $\"{Pattern}\";");
            Next?.GenerateCode(method, writer);
        }
    }
}