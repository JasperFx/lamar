using System;

namespace LamarCodeGeneration.Model;

public interface IVariableSource
{
    bool Matches(Type type);
    Variable Create(Type type);
}