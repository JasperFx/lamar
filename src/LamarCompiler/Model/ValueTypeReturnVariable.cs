using System;
using System.Linq;
using LamarCompiler.Util;

namespace LamarCompiler.Model
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