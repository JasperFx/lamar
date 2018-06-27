using System;
using System.Collections.Generic;

namespace Lamar.IoC.Exports
{
    public interface IResolverLoader
    {
        Dictionary<string, Type> ResolverTypes();
    }
}