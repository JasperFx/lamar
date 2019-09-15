using System;

namespace Lamar.IoC
{
    public class ContainerValidationException : Exception
    {
        /// <summary>
        /// A textual report of the container configuration at the point the exception was thrown
        /// </summary>
        public string WhatDoIHave { get; }
        
        /// <summary>
        /// A textual report of the container type scanning at the point the exception was thrown
        /// </summary>
        public string WhatDidIScan { get; }

        public ContainerValidationException(string message, string whatDoIHave, string whatDidIScan) : base(message)
        {
            WhatDoIHave = whatDoIHave;
            WhatDidIScan = whatDidIScan;
        }

    }
}