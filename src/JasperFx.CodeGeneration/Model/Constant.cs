using System;

namespace JasperFx.CodeGeneration.Model;

public static class Constant
{
    public static Variable For(object value)
    {
        if (value == null)
        {
            return new Variable(typeof(void), "null");
        }

        return new Variable(value.GetType(), CodeFormatter.Write(value));
    }

    public static Variable ForEnum<T>(T value) where T : Enum
    {
        return new Variable(typeof(T), CodeFormatter.Write(value));
    }

    public static Variable ForString(string value)
    {
        return new Variable(typeof(string), CodeFormatter.Write(value));
    }

    public static Variable ForType(Type type)
    {
        return new Variable(typeof(Type), CodeFormatter.Write(type));
    }
}