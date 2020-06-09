using System.Collections.Generic;
using System.Linq;
using LamarCodeGeneration.Model;
using LamarCodeGeneration.Util;

namespace LamarCodeGeneration.Frames
{
    public interface ICodeFrame
    {
        /// <summary>
        /// Mark this variable as being created by the CodeFrame
        /// </summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        ICodeFrame Creates(Variable variable);
    }
    
    public class CodeFrame : Frame, ICodeFrame
    {
        private readonly string _format;
        private readonly object[] _values;

        public CodeFrame(bool isAsync, string format, params object[] values) : base(isAsync)
        {
            _format = format;
            _values = values;

            // For dependency ordering later
            uses.AddRange(values.OfType<Variable>());

        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            var substitutions = _values.Select(CodeFormatter.Write).ToArray();
            var code = string.Format(_format, substitutions);
            
            writer.WriteLine(code);
            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            for (int i = 0; i < _values.Length; i++)
            {
                if (_values[i] is Use u)
                {
                    var variable = u.FindVariable(chain);
                    yield return variable;
                    _values[i] = variable;
                }
            }
        }

        ICodeFrame ICodeFrame.Creates(Variable variable)
        {
            creates.Add(variable);
            return this;
        }
    }
}