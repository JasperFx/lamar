﻿using System;
using System.Linq;
using JasperFx.Core;
using Lamar.IoC.Resolvers;
using LamarCodeGeneration;
using LamarCodeGeneration.Util;

namespace Lamar.IoC.Instances
{
    public class ErrorMessageResolver : IResolver
    {
        private readonly string _message;

        public ErrorMessageResolver(Instance instance)
        {
            ServiceType = instance.ServiceType;
            Name = instance.Name;
            Hash = instance.GetHashCode();

            var dependencyProblems = instance.Dependencies.SelectMany(dep =>
                {
                    return dep.ErrorMessages.Select(x => $"Dependency {dep}: {x}");
                });
            
            _message = instance.ErrorMessages.Concat(dependencyProblems).Join(Environment.NewLine);
        }

        public object Resolve(Scope scope)
        {
            throw new LamarException($"Cannot build registered instance {Name} of '{ServiceType.FullNameInCode()}':{Environment.NewLine}{_message}");
        }

        public Type ServiceType { get; }
        public string Name { get; set; }
        public int Hash { get; set; }
    }
}