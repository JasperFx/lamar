using System;
using Lamar.Codegen.Frames;

namespace Lamar.Codegen.Variables
{
    public class OutArgument : Variable
    {
        public OutArgument(Type variableType) : base(variableType)
        {
        }

        public OutArgument(Type variableType, string usage) : base(variableType, usage)
        {
        }

        public OutArgument(Type variableType, string usage, Frame creator) : base(variableType, usage, creator)
        {
        }

        public OutArgument(Type variableType, Frame creator) : base(variableType, creator)
        {
        }

        public override string ArgumentDeclaration => $"out var {Usage}";
    }
}