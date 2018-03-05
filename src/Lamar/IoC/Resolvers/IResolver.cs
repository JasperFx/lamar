using System;

namespace Lamar.IoC.Resolvers
{
    public interface IResolver
    {
        object Resolve(Scope scope);
        Type ServiceType { get; }
        
        string Name { get; set; }
        int Hash { get; set; }
        
    }
}