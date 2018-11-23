using System;
using System.ComponentModel;
using LamarCompiler.Frames;
using LamarCompiler.Model;

namespace Lamar.IoC.Instances
{
    public class Value : Variable
    {
        public static string RepresentationInCode(object value)
        {
            if (value is string s) return $"\"{value}\"";
            if (value is int i) return value.ToString();
            if (value is double d) return value.ToString();
            
            throw new NotSupportedException($"The Value placeholder does not (yet) support values of type {value.GetType()}");
        }
        
        public Value(object value) : base(value.GetType(), RepresentationInCode(value))
        {
            
        }
    }
}