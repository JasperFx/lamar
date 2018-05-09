using System;
using System.Linq;
using Lamar.Util;

namespace Lamar.Codegen.Variables
{
    public class ValueTypeReturnVariable : Variable
    {
        private readonly Variable[] _inner;

        public ValueTypeReturnVariable(Type returnType, Variable[] inner) : base(returnType)
        {
            _inner = inner;
        }

        public override string Usage => "(" + _inner.Select(x => $"var {x.Usage}").Join(", ") + ")";
    }
}