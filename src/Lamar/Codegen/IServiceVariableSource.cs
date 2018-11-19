using Lamar.Codegen.Variables;

namespace Lamar.Codegen
{
    public interface IServiceVariableSource : IVariableSource
    {
        void ReplaceVariables();
    }
}