using System;

namespace LamarCodeGeneration.Model
{
    public static class Constant
    {
        public static Variable ForEnum<T>(T value) where T : Enum
        {
            return new Variable(typeof(T), typeof(T).FullNameInCode() + "." + value);
        }

        public static Variable ForString(string value)
        {
            return new Variable(typeof(string), "\"" + value + "\"");
        }
    }
}