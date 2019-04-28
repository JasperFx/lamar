using System;
using System.Collections.Generic;
using System.Reflection;

namespace Lamar.AutoFactory
{
    public interface IAutoFactoryConventionProvider
    {
        // SAMPLE: GetMethodDefinition
        IAutoFactoryMethodDefinition GetMethodDefinition(MethodInfo methodInfo, IList<object> arguments);

        //ENDSAMPLE
    }
}