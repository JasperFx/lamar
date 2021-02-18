using System;
using System.Collections.Generic;
using Lamar.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace Lamar.Testing.Diagnostics
{
    public class LamarServicesCommandTests
    {
        [Theory]
        [InlineData(typeof(LamarServicesCommandTests), "Lamar.Testing")]
        [InlineData(typeof(MyThing), "Lamar.Testing")]
        [InlineData(typeof(IEnumerable<MyThing>), "Lamar.Testing")]
        [InlineData(typeof(IOptions<MyThing>), "Lamar.Testing")]
        [InlineData(typeof(ILogger<MyThing>), "Lamar.Testing")]
        public void assembly_from_type(Type type, string assemblyName)
        {
            LamarServicesCommand.AssemblyForType(type).GetName().Name.ShouldBe(assemblyName);
        }
        
        
    }
    
    public class MyThing{}
}