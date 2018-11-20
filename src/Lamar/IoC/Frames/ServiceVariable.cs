using System;
using Lamar.IoC.Instances;
using LamarCompiler.Frames;
using LamarCompiler.Model;

namespace Lamar.IoC.Frames
{
    public enum ServiceDeclaration
    {
        ImplementationType,
        ServiceType
    }
    
    public class ServiceVariable : Variable, IServiceVariable
    {
        public ServiceVariable(Instance instance, Frame creator, ServiceDeclaration declaration = ServiceDeclaration.ImplementationType) 
            : base(declaration == ServiceDeclaration.ImplementationType ? instance.ImplementationType : instance.ServiceType, instance.Name.Sanitize(), creator)
        {
            Instance = instance;
        }
        
        public Instance Instance { get; }
    }
}