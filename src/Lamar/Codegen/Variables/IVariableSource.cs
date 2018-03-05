using System;

namespace Lamar.Codegen.Variables
{
    public interface IVariableSource
    {
        bool Matches(Type type);
        Variable Create(Type type);
    }
}
