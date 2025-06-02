using System.Collections.Generic;
using System;
using System.Linq;
using JasperFx.Core.Reflection;

namespace Lamar.IoC;

public class LamarInstanceHashCollisionException : LamarException
{
    public int InstanceHash { get; }
    public IEnumerable<Type> ServiceTypes { get; }

    public LamarInstanceHashCollisionException(int instanceHash, IEnumerable<Type> serviceTypes) : base(
        $"Duplicate hash '{instanceHash}' generated for services: {string.Join(", ", serviceTypes.Select(x => x.FullNameInCode()))}")
    {
        InstanceHash = instanceHash;
        ServiceTypes = serviceTypes;
    }
}