using System;
using LamarCodeGeneration.Model;

namespace LamarCodeGeneration
{
    public static class CodeFormatter
    {
        public static string Write(object value)
        {
            if (value == null) return "null";

            if (value is Variable v) return v.Usage;
            
            if (value is string)
            {
                return "\"" + value + "\"";
            }

            if (value.GetType().IsEnum)
            {
                return value.GetType().FullNameInCode() + "." + value;
            }

            if (value is Type t) return $"typeof({t.FullNameInCode()})";

            return value.ToString();
        }
    }
}