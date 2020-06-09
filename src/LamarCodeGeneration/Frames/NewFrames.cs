using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using LamarCodeGeneration.Model;

namespace LamarCodeGeneration.Frames
{
    public class NotImplementedFrame: SyncFrame
    {
        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteLine($"throw new {typeof(NotImplementedException).FullNameInCode()}();");
        }
    }
    
    public class NotSupportedFrame: SyncFrame
    {
        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteLine($"throw new {typeof(NotSupportedException).FullNameInCode()}();");
        }
    }
    
    public class ReturnPropertyFrame: SyncFrame
    {
        private readonly Type _variableType;
        private readonly MemberInfo _member;
        private Variable _variable;

        public ReturnPropertyFrame(Type variableType, MemberInfo member)
        {
            _variableType = variableType;
            _member = member;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteLine($"return {_variable.Usage}.{_member.Name};");
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            _variable = chain.FindVariable(_variableType);
            yield return _variable;
        }
    }

    // TODO -- this should be in LamarCodeGeneration
    public class ReturnTaskCompleted: Frame
    {
        public ReturnTaskCompleted() : base(true)
        {

        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteLine($"return {typeof(Task).FullNameInCode()}.{nameof(Task.CompletedTask)};");
        }
    }
    
    /*
        private static void writeNotImplementedStubs(GeneratedType type)
        {
            foreach (var method in type.Methods)
            {
                if (!method.Frames.Any())
                {
                    method.Frames.Add(new NotImplementedFrame());
                }
            }
        }
     */
}