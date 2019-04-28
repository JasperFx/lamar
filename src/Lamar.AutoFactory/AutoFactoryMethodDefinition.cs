using System;

namespace Lamar.AutoFactory
{
    public class AutoFactoryMethodDefinition : IAutoFactoryMethodDefinition
    {
        public AutoFactoryMethodDefinition(AutoFactoryMethodType methodType, Type instanceType, string instanceName)
        {
            MethodType = methodType;
            InstanceType = instanceType;
            InstanceName = instanceName;
        }

        public AutoFactoryMethodType MethodType { get; }

        public Type InstanceType { get; }

        public string InstanceName { get; }
    }
}