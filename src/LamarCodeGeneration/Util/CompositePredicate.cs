using System;
using System.Collections.Generic;
using System.Linq;

namespace LamarCodeGeneration.Util
{
    internal class CompositePredicate<T>
    {
        private readonly List<Func<T, bool>> _list = new List<Func<T, bool>>();
        private Func<T, bool> _matchesAll = x => true;
        private Func<T, bool> _matchesAny = x => true;
        private Func<T, bool> _matchesNone = x => false;

        internal void Add(Func<T, bool> filter)
        {
            _matchesAll = x => _list.All(predicate => predicate(x));
            _matchesAny = x => _list.Any(predicate => predicate(x));
            _matchesNone = x => !MatchesAny(x);

            _list.Add(filter);
        }



        public static CompositePredicate<T> operator +(CompositePredicate<T> invokes, Func<T, bool> filter)
        {
            invokes.Add(filter);
            return invokes;
        }

        internal bool MatchesAll(T target)
        {
            return _matchesAll(target);
        }

        internal bool MatchesAny(T target)
        {
            return _matchesAny(target);
        }

        internal bool MatchesNone(T target)
        {
            return _matchesNone(target);
        }

        internal bool DoesNotMatcheAny(T target)
        {
            return _list.Count == 0 || !MatchesAny(target);
        }
    }
}
