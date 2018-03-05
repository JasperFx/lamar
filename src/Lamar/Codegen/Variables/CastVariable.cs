using System;

namespace Lamar.Codegen.Variables
{
    public class CastVariable : Variable
    {
        public CastVariable(Variable parent, Type specificType) : base(specificType,
            $"(({specificType.FullNameInCode()}){parent.Usage})")
        {
            Dependencies.Add(parent);
            Inner = parent;
        }

        // strictly for easier testing
        public Variable Inner { get; }
    }
}