using System;

namespace JasperFx.CodeGeneration.Model;

public interface IVariableSource
{
    bool Matches(Type type);
    Variable Create(Type type);
}