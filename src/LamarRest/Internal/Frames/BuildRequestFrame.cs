using System.Net.Http;
using LamarCodeGeneration.Frames;

namespace LamarRest.Internal.Frames
{
    /// <summary>
    /// Builds out the HttpRequestMessage input
    /// </summary>
    public class BuildRequestFrame : ConstructorFrame<HttpRequestMessage>
    {
        public BuildRequestFrame(string httpMethod, FillUrlFrame urlFrame, SerializeJsonFrame serializedInput) : base(() => new HttpRequestMessage(null, ""))
        {
            Parameters[0] = new HttpMethodVariable(httpMethod);
            Parameters[1] = urlFrame.Url;

            if (serializedInput != null)
            {
                Set(x => x.Content, new JsonContentVariable(serializedInput.Json));
            }
        }
    }
}