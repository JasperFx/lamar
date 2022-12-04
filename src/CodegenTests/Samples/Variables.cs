using System;
using CodegenTests.Codegen;
using JasperFx.CodeGeneration.Model;
using Shouldly;
using Xunit;
#if NETCOREAPP2_0
using Microsoft.AspNetCore.Http;
#endif

namespace CodegenTests.Samples
{
    public interface IService
    {
    }

    public class WhateverService : IService
    {
    }

    public class SqlConnection
    {
    }

    public class Variables
    {
        [Fact]
        public void use_variables()
        {
            #region sample_derived-variable

            var now = new Variable(typeof(DateTime), $"{typeof(DateTime).FullName}.{nameof(DateTime.Now)}");

            #endregion

            #region sample_default-variable-name-usage

            var widget = Variable.For<IWidget>();
            widget.Usage.ShouldBe("widget");

            #endregion


            #region sample_override-variable-usage-and-type

            var service = new Variable(typeof(IService), "service");
            service.OverrideName("myService");
            service.OverrideType(typeof(WhateverService));

            #endregion


            #region sample_create-a-variable

            // Create a connection for the type SqlConnection 
            // with the name "conn"
            var conn = Variable.For<SqlConnection>("conn");

            // Pretty well the same thing above
            var conn2 = new Variable(typeof(SqlConnection), "conn2");

            // Create a variable with the default name
            // for the type
            var conn3 = Variable.For<SqlConnection>();
            conn3.Usage.ShouldBe("sqlConnection");

            #endregion


#if NETCOREAPP2_0
            #region sample_variable-dependencies
            var context = Variable.For<HttpContext>();
            var response = new Variable(typeof(HttpResponse), $"{context.Usage}.{nameof(HttpContext.Response)}");
            response.Dependencies.Add(context);
            #endregion
#endif
        }
    }
}