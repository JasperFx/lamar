using System;
using LamarCodeGeneration.Model;
using LamarCompiler.Testing.Codegen;
using Shouldly;
using Xunit;
#if NETCOREAPP2_0
using Microsoft.AspNetCore.Http;
#endif

namespace LamarCompiler.Testing.Samples
{
    public interface IService{}
    public class WhateverService : IService{}
    
    public class SqlConnection{}
    
    public class Variables
    {
        [Fact]
        public void use_variables()
        {
            // SAMPLE: derived-variable
            var now = new Variable(typeof(DateTime), $"{typeof(DateTime).FullName}.{nameof(DateTime.Now)}");
            // ENDSAMPLE

            // SAMPLE: default-variable-name-usage
            var widget = Variable.For<IWidget>();
            widget.Usage.ShouldBe("widget");
            // ENDSAMPLE
            
            
            
            // SAMPLE: override-variable-usage-and-type
            var service = new Variable(typeof(IService), "service");
            service.OverrideName("myService");
            service.OverrideType(typeof(WhateverService));
            // ENDSAMPLE


            // SAMPLE: create-a-variable
            // Create a connection for the type SqlConnection 
            // with the name "conn"
            var conn = Variable.For<SqlConnection>("conn");
            
            // Pretty well the same thing above
            var conn2 = new Variable(typeof(SqlConnection), "conn2");

            // Create a variable with the default name
            // for the type
            var conn3 = Variable.For<SqlConnection>();
            conn3.Usage.ShouldBe("sqlConnection");
            // ENDSAMPLE


#if NETCOREAPP2_0
            // SAMPLE: variable-dependencies
            var context = Variable.For<HttpContext>();
            var response = new Variable(typeof(HttpResponse), $"{context.Usage}.{nameof(HttpContext.Response)}");
            response.Dependencies.Add(context);
            // ENDSAMPLE 
#endif

        }
    }
}