using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lamar.IoC.Frames;
using LamarCompiler.Frames;
using LamarCompiler.Model;
using LamarCompiler.Util;

namespace Lamar.IoC.Instances
{
    public class InstanceConstructorFrame : ConstructorFrame
    {
        public InstanceConstructorFrame(ConstructorInstance instance, DisposeTracking disposal, Variable[] arguments,
            SetterArg[] setterParameters) : base(instance.ImplementationType, instance.Constructor, f => new ServiceVariable(instance, f, ServiceDeclaration.ServiceType))
        {
            Disposal = disposal;

            Parameters = arguments;
            
            switch (disposal)
            {
                case DisposeTracking.None:
                    Mode = ConstructorCallMode.Variable;
                    break;
                
                case DisposeTracking.WithUsing:
                    Mode = ConstructorCallMode.UsingNestedVariable;
                    break;
                
                case DisposeTracking.RegisterWithScope:
                    Mode = ConstructorCallMode.Variable;
                    var addDisposable = MethodCall.For<Scope>(s => s.TryAddDisposable(null));
                    addDisposable.Arguments[0] = Variable;
                    ActivatorFrames.Add(addDisposable);
                    break;
            }

            Setters.AddRange(setterParameters);
        }

        public DisposeTracking Disposal { get; set; }
    }
}