// Taken from https://github.com/dadhi/ImTools

/*
The MIT License (MIT)

Copyright (c) 2016-2017 Maksim Volkau

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included AddOrUpdateServiceFactory
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace LamarCodeGeneration.Util
{ // For [MethodImpl(AggressiveInlining)]



    /// <summary>Immutable Key-Value pair. It is reference type (could be check for null), 
    /// which is different from System value type <see cref="KeyValuePair{TKey,TValue}"/>.
    /// In addition provides <see cref="Equals"/> and <see cref="GetHashCode"/> implementations.</summary>
    /// <typeparam name="K">Type of Key.</typeparam><typeparam name="V">Type of Value.</typeparam>
    internal class KV<K, V>
    {
        /// <summary>Key.</summary>
        public readonly K Key;

        /// <summary>Value.</summary>
        public readonly V Value;

        /// <summary>Creates Key-Value object by providing key and value. Does Not check either one for null.</summary>
        /// <param name="key">key.</param><param name="value">value.</param>
        public KV(K key, V value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>Creates nice string view.</summary><returns>String representation.</returns>
        public override string ToString()
        {
            var s = new StringBuilder('{');
            if (Key != null)
                s.Append(Key);
            s.Append(',');
            if (Value != null)
                s.Append(Value);
            s.Append('}');
            return s.ToString();
        }

        /// <summary>Returns true if both key and value are equal to corresponding key-value of other object.</summary>
        /// <param name="obj">Object to check equality with.</param> <returns>True if equal.</returns>
        public override bool Equals(object obj)
        {
            var other = obj as KV<K, V>;
            return other != null
                   && (ReferenceEquals(other.Key, Key) || Equals(other.Key, Key))
                   && (ReferenceEquals(other.Value, Value) || Equals(other.Value, Value));
        }

        /// <summary>Combines key and value hash code. R# generated default implementation.</summary>
        /// <returns>Combined hash code for key-value.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((object)Key == null ? 0 : Key.GetHashCode() * 397)
                       ^ ((object)Value == null ? 0 : Value.GetHashCode());
            }
        }
    }


    /// <summary>Given the old value should and the new value should return result updated value.</summary>
    internal delegate V Update<V>(V oldValue, V newValue);


    /// <summary>Immutable http://en.wikipedia.org/wiki/AVL_tree 
    /// where node key is the hash code of <typeparamref name="K"/>.</summary>
    internal sealed class ImHashMap<K, V>
    {
        /// <summary>Empty tree to start with.</summary>
        public static readonly ImHashMap<K, V> Empty = new ImHashMap<K, V>();

        /// <summary>Calculated key hash.</summary>
        public int Hash
        {
            get { return _data.Hash; }
        }

        /// <summary>Key of type K that should support <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode"/>.</summary>
        public K Key
        {
            get { return _data.Key; }
        }

        /// <summary>Value of any type V.</summary>
        public V Value
        {
            get { return _data.Value; }
        }

        /// <summary>In case of <see cref="Hash"/> conflicts for different keys contains conflicted keys with their values.</summary>
        public KV<K, V>[] Conflicts
        {
            get { return _data.Conflicts; }
        }

        /// <summary>Left sub-tree/branch, or empty.</summary>
        public readonly ImHashMap<K, V> Left;

        /// <summary>Right sub-tree/branch, or empty.</summary>
        public readonly ImHashMap<K, V> Right;

        /// <summary>Height of longest sub-tree/branch plus 1. It is 0 for empty tree, and 1 for single node tree.</summary>
        public readonly int Height;

        /// <summary>Returns true if tree is empty.</summary>
        public bool IsEmpty
        {
            get { return Height == 0; }
        }

        /// <summary>Returns new tree with added key-value. 
        /// If value with the same key is exist then the value is replaced.</summary>
        /// <param name="key">Key to add.</param><param name="value">Value to add.</param>
        /// <returns>New tree with added or updated key-value.</returns>
        public ImHashMap<K, V> AddOrUpdate(K key, V value)
        {
            return AddOrUpdate(key.GetHashCode(), key, value);
        }

        /// <summary>Returns new tree with added key-value. If value with the same key is exist, then
        /// if <paramref name="update"/> is not specified: then existing value will be replaced by <paramref name="value"/>;
        /// if <paramref name="update"/> is specified: then update delegate will decide what value to keep.</summary>
        /// <param name="key">Key to add.</param><param name="value">Value to add.</param>
        /// <param name="update">Update handler.</param>
        /// <returns>New tree with added or updated key-value.</returns>
        public ImHashMap<K, V> AddOrUpdate(K key, V value, Update<V> update)
        {
            return AddOrUpdate(key.GetHashCode(), key, value, update);
        }

        /// <summary>Looks for <paramref name="key"/> and replaces its value with new <paramref name="value"/>, or 
        /// runs custom update handler (<paramref name="update"/>) with old and new value to get the updated result.</summary>
        /// <param name="key">Key to look for.</param>
        /// <param name="value">New value to replace key value with.</param>
        /// <param name="update">(optional) Delegate for custom update logic, it gets old and new <paramref name="value"/>
        /// as inputs and should return updated value as output.</param>
        /// <returns>New tree with updated value or the SAME tree if no key found.</returns>
        public ImHashMap<K, V> Update(K key, V value, Update<V> update = null)
        {
            return Update(key.GetHashCode(), key, value, update);
        }

        /// <summary>Looks for key in a tree and returns the key value if found, or <paramref name="defaultValue"/> otherwise.</summary>
        /// <param name="key">Key to look for.</param> <param name="defaultValue">(optional) Value to return if key is not found.</param>
        /// <returns>Found value or <paramref name="defaultValue"/>.</returns>
        [MethodImpl((MethodImplOptions)256)]
        public V GetValueOrDefault(K key, V defaultValue = default(V))
        {
            var t = this;
            var hash = key.GetHashCode();
            while (t.Height != 0 && t.Hash != hash)
                t = hash < t.Hash ? t.Left : t.Right;
            return t.Height != 0 && (ReferenceEquals(key, t.Key) || key.Equals(t.Key))
                ? t.Value : t.GetConflictedValueOrDefault(key, defaultValue);
        }

        /// <summary>Returns true if key is found and sets the value.</summary>
        /// <param name="key">Key to look for.</param> <param name="value">Result value</param>
        /// <returns>True if key found, false otherwise.</returns>
        [MethodImpl((MethodImplOptions)256)]
        public bool TryFind(K key, out V value)
        {
            var hash = key.GetHashCode();

            var t = this;
            while (t.Height != 0 && t._data.Hash != hash)
                t = hash < t._data.Hash ? t.Left : t.Right;

            if (t.Height != 0 && (ReferenceEquals(key, t._data.Key) || key.Equals(t._data.Key)))
            {
                value = t._data.Value;
                return true;
            }

            return t.TryFindConflictedValue(key, out value);
        }

        /// <summary>Depth-first in-order traversal as described in http://en.wikipedia.org/wiki/Tree_traversal
        /// The only difference is using fixed size array instead of stack for speed-up (~20% faster than stack).</summary>
        /// <returns>Sequence of enumerated key value pairs.</returns>
        public IEnumerable<KV<K, V>> Enumerate()
        {
            if (Height == 0)
                yield break;

            var parents = new ImHashMap<K, V>[Height];

            var node = this;
            var parentCount = -1;
            while (node.Height != 0 || parentCount != -1)
            {
                if (node.Height != 0)
                {
                    parents[++parentCount] = node;
                    node = node.Left;
                }
                else
                {
                    node = parents[parentCount--];
                    yield return new KV<K, V>(node.Key, node.Value);

                    if (node.Conflicts != null)
                        for (var i = 0; i < node.Conflicts.Length; i++)
                            yield return node.Conflicts[i];

                    node = node.Right;
                }
            }
        }

        /// <summary>Removes or updates value for specified key, or does nothing if key is not found.
        /// Based on Eric Lippert http://blogs.msdn.com/b/ericlippert/archive/2008/01/21/immutability-in-c-part-nine-academic-plus-my-avl-tree-implementation.aspx </summary>
        /// <param name="key">Key to look for.</param> 
        /// <returns>New tree with removed or updated value.</returns>
        public ImHashMap<K, V> Remove(K key)
        {
            return Remove(key.GetHashCode(), key);
        }

        #region Implementation

        private sealed class Data
        {
            public readonly int Hash;
            public readonly K Key;
            public readonly V Value;

            public readonly KV<K, V>[] Conflicts;

            public Data() { }

            public Data(int hash, K key, V value, KV<K, V>[] conflicts = null)
            {
                Hash = hash;
                Key = key;
                Value = value;
                Conflicts = conflicts;
            }
        }

        private readonly Data _data;

        private ImHashMap() { _data = new Data(); }

        private ImHashMap(Data data)
        {
            _data = data;
            Left = Empty;
            Right = Empty;
            Height = 1;
        }

        private ImHashMap(Data data, ImHashMap<K, V> left, ImHashMap<K, V> right)
        {
            _data = data;
            Left = left;
            Right = right;
            Height = 1 + (left.Height > right.Height ? left.Height : right.Height);
        }

        private ImHashMap(Data data, ImHashMap<K, V> left, ImHashMap<K, V> right, int height)
        {
            _data = data;
            Left = left;
            Right = right;
            Height = height;
        }

        internal ImHashMap<K, V> AddOrUpdate(int hash, K key, V value)
        {
            return Height == 0  // add new node
                ? new ImHashMap<K, V>(new Data(hash, key, value))
                : (hash == Hash // update found node
                    ? (ReferenceEquals(Key, key) || Key.Equals(key)
                        ? new ImHashMap<K, V>(new Data(hash, key, value, Conflicts), Left, Right)
                        : UpdateValueAndResolveConflicts(key, value, null, false))
                    : (hash < Hash  // search for node
                        ? (Height == 1
                            ? new ImHashMap<K, V>(_data,
                                new ImHashMap<K, V>(new Data(hash, key, value)), Right, height: 2)
                            : new ImHashMap<K, V>(_data,
                                Left.AddOrUpdate(hash, key, value), Right).KeepBalance())
                        : (Height == 1
                            ? new ImHashMap<K, V>(_data,
                                Left, new ImHashMap<K, V>(new Data(hash, key, value)), height: 2)
                            : new ImHashMap<K, V>(_data,
                                Left, Right.AddOrUpdate(hash, key, value)).KeepBalance())));
        }

        private ImHashMap<K, V> AddOrUpdate(int hash, K key, V value, Update<V> update)
        {
            return Height == 0
                ? new ImHashMap<K, V>(new Data(hash, key, value))
                : (hash == Hash // update
                    ? (ReferenceEquals(Key, key) || Key.Equals(key)
                        ? new ImHashMap<K, V>(new Data(hash, key, update(Value, value), Conflicts), Left, Right)
                        : UpdateValueAndResolveConflicts(key, value, update, false))
                    : (hash < Hash
                        ? With(Left.AddOrUpdate(hash, key, value, update), Right)
                        : With(Left, Right.AddOrUpdate(hash, key, value, update)))
                    .KeepBalance());
        }

        internal ImHashMap<K, V> Update(int hash, K key, V value, Update<V> update)
        {
            return Height == 0 ? this
                : (hash == Hash
                    ? (ReferenceEquals(Key, key) || Key.Equals(key)
                        ? new ImHashMap<K, V>(new Data(hash, key, update == null ? value : update(Value, value), Conflicts), Left, Right)
                        : UpdateValueAndResolveConflicts(key, value, update, true))
                    : (hash < Hash
                        ? With(Left.Update(hash, key, value, update), Right)
                        : With(Left, Right.Update(hash, key, value, update)))
                    .KeepBalance());
        }

        private ImHashMap<K, V> UpdateValueAndResolveConflicts(K key, V value, Update<V> update, bool updateOnly)
        {
            if (Conflicts == null) // add only if updateOnly is false.
                return updateOnly ? this
                    : new ImHashMap<K, V>(new Data(Hash, Key, Value, new[] { new KV<K, V>(key, value) }), Left, Right);

            var found = Conflicts.Length - 1;
            while (found >= 0 && !Equals(Conflicts[found].Key, Key)) --found;
            if (found == -1)
            {
                if (updateOnly) return this;
                var newConflicts = new KV<K, V>[Conflicts.Length + 1];
                Array.Copy(Conflicts, 0, newConflicts, 0, Conflicts.Length);
                newConflicts[Conflicts.Length] = new KV<K, V>(key, value);
                return new ImHashMap<K, V>(new Data(Hash, Key, Value, newConflicts), Left, Right);
            }

            var conflicts = new KV<K, V>[Conflicts.Length];
            Array.Copy(Conflicts, 0, conflicts, 0, Conflicts.Length);
            conflicts[found] = new KV<K, V>(key, update == null ? value : update(Conflicts[found].Value, value));
            return new ImHashMap<K, V>(new Data(Hash, Key, Value, conflicts), Left, Right);
        }

        internal V GetConflictedValueOrDefault(K key, V defaultValue)
        {
            if (Conflicts != null)
                for (var i = Conflicts.Length - 1; i >= 0; --i)
                    if (Equals(Conflicts[i].Key, key))
                        return Conflicts[i].Value;
            return defaultValue;
        }

        private bool TryFindConflictedValue(K key, out V value)
        {
            if (Height != 0 && Conflicts != null)
                for (var i = Conflicts.Length - 1; i >= 0; --i)
                    if (Equals(Conflicts[i].Key, key))
                    {
                        value = Conflicts[i].Value;
                        return true;
                    }

            value = default(V);
            return false;
        }

        private ImHashMap<K, V> KeepBalance()
        {
            var delta = Left.Height - Right.Height;
            if (delta >= 2) // left is longer by 2, rotate left
            {
                var left = Left;
                var leftLeft = left.Left;
                var leftRight = left.Right;
                if (leftRight.Height - leftLeft.Height == 1)
                {
                    // double rotation:
                    //      5     =>     5     =>     4
                    //   2     6      4     6      2     5
                    // 1   4        2   3        1   3     6
                    //    3        1
                    return new ImHashMap<K, V>(leftRight._data,
                        left: new ImHashMap<K, V>(left._data,
                            left: leftLeft, right: leftRight.Left), right: new ImHashMap<K, V>(_data,
                            left: leftRight.Right, right: Right));
                }

                // todo: do we need this?
                // one rotation:
                //      5     =>     2
                //   2     6      1     5
                // 1   4              4   6
                return new ImHashMap<K, V>(left._data,
                    left: leftLeft, right: new ImHashMap<K, V>(_data,
                        left: leftRight, right: Right));
            }

            if (delta <= -2)
            {
                var right = Right;
                var rightLeft = right.Left;
                var rightRight = right.Right;
                if (rightLeft.Height - rightRight.Height == 1)
                {
                    return new ImHashMap<K, V>(rightLeft._data,
                        left: new ImHashMap<K, V>(_data,
                            left: Left, right: rightLeft.Left), right: new ImHashMap<K, V>(right._data,
                            left: rightLeft.Right, right: rightRight));
                }

                return new ImHashMap<K, V>(right._data,
                    left: new ImHashMap<K, V>(_data,
                        left: Left, right: rightLeft), right: rightRight);
            }

            return this;
        }

        private ImHashMap<K, V> With(ImHashMap<K, V> left, ImHashMap<K, V> right)
        {
            return left == Left && right == Right ? this : new ImHashMap<K, V>(_data, left, right);
        }

        internal ImHashMap<K, V> Remove(int hash, K key, bool ignoreKey = false)
        {
            if (Height == 0)
                return this;

            ImHashMap<K, V> result;
            if (hash == Hash) // found node
            {
                if (ignoreKey || Equals(Key, key))
                {
                    if (!ignoreKey && Conflicts != null)
                        return ReplaceRemovedWithConflicted();

                    if (Height == 1) // remove node
                        return Empty;

                    if (Right.IsEmpty)
                        result = Left;
                    else if (Left.IsEmpty)
                        result = Right;
                    else
                    {
                        // we have two children, so remove the next highest node and replace this node with it.
                        var successor = Right;
                        while (!successor.Left.IsEmpty) successor = successor.Left;
                        result = new ImHashMap<K, V>(successor._data,
                            Left, Right.Remove(successor.Hash, default(K), ignoreKey: true));
                    }
                }
                else if (Conflicts != null)
                    return TryRemoveConflicted(key);
                else
                    return this; // if key is not matching and no conflicts to lookup - just return
            }
            else if (hash < Hash)
                result = new ImHashMap<K, V>(_data, Left.Remove(hash, key, ignoreKey), Right);
            else
                result = new ImHashMap<K, V>(_data, Left, Right.Remove(hash, key, ignoreKey));

            if (result.Height == 1)
                return result;

            return result.KeepBalance();
        }

        private ImHashMap<K, V> TryRemoveConflicted(K key)
        {
            var index = Conflicts.Length - 1;
            while (index >= 0 && !Equals(Conflicts[index].Key, key)) --index;
            if (index == -1) // key is not found in conflicts - just return
                return this;

            if (Conflicts.Length == 1)
                return new ImHashMap<K, V>(new Data(Hash, Key, Value), Left, Right);
            var shrinkedConflicts = new KV<K, V>[Conflicts.Length - 1];
            var newIndex = 0;
            for (var i = 0; i < Conflicts.Length; ++i)
                if (i != index) shrinkedConflicts[newIndex++] = Conflicts[i];
            return new ImHashMap<K, V>(new Data(Hash, Key, Value, shrinkedConflicts), Left, Right);
        }

        private ImHashMap<K, V> ReplaceRemovedWithConflicted()
        {
            if (Conflicts.Length == 1)
                return new ImHashMap<K, V>(new Data(Hash, Conflicts[0].Key, Conflicts[0].Value), Left, Right);
            var shrinkedConflicts = new KV<K, V>[Conflicts.Length - 1];
            Array.Copy(Conflicts, 1, shrinkedConflicts, 0, shrinkedConflicts.Length);
            return new ImHashMap<K, V>(new Data(Hash, Conflicts[0].Key, Conflicts[0].Value, shrinkedConflicts), Left, Right);
        }

        #endregion
    }
}