using System;

namespace Lamar.IoC.Resolvers;

public interface IResolver
{
    Type ServiceType { get; }

    string Name { get; set; }
    int Hash { get; set; }
    object Resolve(Scope scope);
}