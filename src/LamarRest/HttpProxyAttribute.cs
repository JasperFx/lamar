using System;

namespace LamarRest
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HttpProxyAttribute : Attribute
    {
        public string ConfigKey { get; }

        public HttpProxyAttribute()
        {
        }

        public HttpProxyAttribute(string configKey)
        {
            ConfigKey = configKey;
        }
    }
}