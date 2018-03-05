using System;

namespace Lamar.Scanning
{
    [Flags]
    public enum TypeClassification : short
    {
        All = 0,
        Open = 1,
        Closed = 2,
        Interfaces = 4,
        Abstracts = 8,
        Concretes = 16
    }
}