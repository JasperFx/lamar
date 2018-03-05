using System;
using System.Collections.Generic;
using System.Linq;
using Lamar.Codegen;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using Lamar.Compilation;
using Lamar.IoC.Enumerables;
using Lamar.Scanning.Conventions;
using Lamar.Util;

namespace Lamar.IoC.Frames
{
    public class ListAssignmentFrame<T> : Frame
    {
        public ListAssignmentFrame(ListInstance<T> instance, Variable[] elements) : base(false)
        {
            ElementType = typeof(T);
            Variable = new ServiceVariable(instance, this);

            if (ElementType.MustBeBuiltWithFunc())
            {
                Variable.OverrideType(typeof(object[]));
            }

            Elements = elements;
        }

        public Type ElementType { get; }

        public Variable[] Elements { get; }

        public Variable Variable { get; }
        public bool ReturnCreated { get; set; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            var declaration = ElementType.MustBeBuiltWithFunc()
                ? "object[]"
                : $"{typeof(List<>).Namespace}.List<{ElementType.FullNameInCode()}>";
            
            var elements = Elements.Select(x => x.Usage).Join(", ");
            if (ReturnCreated)
            {
                writer.Write($"return new {declaration}{{{elements}}};");
            }
            else
            {
                writer.Write($"var {Variable.Usage} = new {declaration}{{{elements}}};");
            }
            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            return Elements;
        }
    }
}
