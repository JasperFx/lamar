using System.Reflection;
using Baseline.Reflection;
using LamarCompiler;
using LamarCompiler.Frames;
using LamarCompiler.Model;

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
            writer.Write($"var {Url.Usage} = $\"{Pattern}\";");
            Next?.GenerateCode(method, writer);
        }
    }
}