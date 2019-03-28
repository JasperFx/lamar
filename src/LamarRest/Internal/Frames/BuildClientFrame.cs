using System;
using System.Collections.Generic;
using System.Net.Http;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using Microsoft.Extensions.Configuration;

namespace LamarRest.Internal.Frames
{
    public class BuildClientFrame : SyncFrame
    {
        private readonly Type _interfaceType;
        private Variable _factory;

        public BuildClientFrame(Type interfaceType)
        {
            _interfaceType = interfaceType;

            Client = new Variable(typeof(HttpClient), this);
        }

        public Variable Client { get; set; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.BlankLine();
            writer.WriteComment($"From {nameof(BuildClientFrame)}");
            writer.Write($"var {Client.Usage} = {_factory.Usage}.{nameof(IHttpClientFactory.CreateClient)}(\"{_interfaceType.Name}\");");
            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            _factory = chain.FindVariable(typeof(IHttpClientFactory));
            yield return _factory;

        }
    }
}