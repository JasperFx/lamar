using System;

namespace LamarRest
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PostAttribute : PathAttribute
    {
        public PostAttribute(string path) : base("POST", path)
        {
        }
    }
}