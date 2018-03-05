using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lamar.Scanning
{
    public class AssemblyShelf
    {
        public readonly IList<Type> Interfaces = new List<Type>();
        public readonly IList<Type> Concretes = new List<Type>();
        public readonly IList<Type> Abstracts = new List<Type>();

        public IEnumerable<IList<Type>> SelectLists(TypeClassification classification)
        {
            var interfaces = classification.HasFlag(TypeClassification.Interfaces);
            var concretes = classification.HasFlag(TypeClassification.Concretes);
            var abstracts = classification.HasFlag(TypeClassification.Abstracts);

            if (interfaces || concretes || abstracts)
            {
                if (interfaces) yield return Interfaces;
                if (concretes) yield return Concretes;
                if (abstracts) yield return Abstracts;
            }
            else
            {
                yield return Interfaces;
                yield return Concretes;
                yield return Abstracts;
            }
        }

        public IEnumerable<Type> AllTypes()
        {
            return Interfaces.Concat(Concretes).Concat(Abstracts);
        }

        public void Add(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsInterface)
            {
                Interfaces.Add(type);
            }
            else if (typeInfo.IsAbstract)
            {
                if (typeInfo.IsSealed)
                {
                    // concrete, static type
                    Concretes.Add(type);
                }
                else
                {
                    Abstracts.Add(type);
                }
            }
            else if (typeInfo.IsClass)
            {
                Concretes.Add(type);
            }
        }
    }
}
