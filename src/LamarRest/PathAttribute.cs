using System;

namespace LamarRest
{
    public abstract class PathAttribute : Attribute
    {
        public string Method { get; }
        public string Path { get; }

        protected PathAttribute(string method, string path)
        {
            Method = method;
            Path = path;
        }
    }
}