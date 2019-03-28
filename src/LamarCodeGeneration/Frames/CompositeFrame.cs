using System.Collections.Generic;
using System.Linq;
using LamarCodeGeneration.Model;

namespace LamarCodeGeneration.Frames
{
    public abstract class CompositeFrame : Frame
    {
        private readonly Frame[] _inner;

        protected CompositeFrame(params Frame[] inner) : base(inner.Any(x => x.IsAsync))
        {
            _inner = inner;
        }

        public override IEnumerable<Variable> Creates => _inner.SelectMany<Frame, Variable>(x => x.Creates).ToArray();
        public sealed override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            if (_inner.Length > 1)
            {
                for (int i = 1; i < _inner.Length; i++)
                {
                    _inner[i - 1].Next = _inner[i];
                }
            }

            generateCode(method, writer, _inner[0]);

            Next?.GenerateCode(method, writer);
        }

        protected abstract void generateCode(GeneratedMethod method, ISourceWriter writer, Frame inner);

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            return _inner.SelectMany(x => x.FindVariables(chain)).Distinct();
        }

        public override bool CanReturnTask()
        {
            return _inner.Last().CanReturnTask();
        }
    }
}
