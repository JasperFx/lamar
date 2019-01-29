using System;

namespace LamarRest
{
    [AttributeUsage(AttributeTargets.Method)]
    public class GetAttribute : PathAttribute
    {
        public GetAttribute(string path) : base("GET", path)
        {
        }
    }
}