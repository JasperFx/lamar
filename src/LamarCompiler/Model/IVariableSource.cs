using System;
using System.Collections.Generic;

namespace LamarCompiler.Model
{
    public interface IVariableSource
    {
        bool Matches(Type type);
        Variable Create(Type type);

    }
}
