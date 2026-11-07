using System;
using System.Collections.Generic;

namespace Embedix.Core.dnlib.Injection.Caching
{
    /// <summary>
    /// Generic memoizing cache used to avoid importing (or otherwise re-computing)
    /// the same dnlib member/signature more than once. Every *Cache field that used
    /// to live directly on InjectMapper is now one instance of this class.
    /// </summary>
    /// <typeparam name="TKey">Type of the source object being imported.</typeparam>
    /// <typeparam name="TValue">Type of the imported result.</typeparam>
    internal sealed class ImportCache<TKey, TValue>
        where TKey : class
        where TValue : class
    {
        private readonly Dictionary<TKey, TValue> _cache = new Dictionary<TKey, TValue>();

        /// <summary>
        /// Returns the cached value for <paramref name="key"/>, or computes it with
        /// <paramref name="factory"/>, stores it, and returns it. Returns null (without
        /// caching) when <paramref name="key"/> is null, mirroring the original null checks.
        /// </summary>
        /// <param name="key">The source object to look up.</param>
        /// <param name="factory">Computes the value when <paramref name="key"/> is not cached yet.</param>
        /// <returns>The cached or newly computed value, or <see langword="null"/> when <paramref name="key"/> is <see langword="null"/>.</returns>
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
        {
            if (key == null) return null;

            if (_cache.TryGetValue(key, out var cached))
                return cached;

            var value = factory(key);
            _cache[key] = value;
            return value;
        }

        /// <summary>
        ///     Tries to get a previously cached value.
        /// </summary>
        /// <param name="key">The source object.</param>
        /// <param name="value">The cached value, or <see langword="null"/> if not found.</param>
        /// <returns><see langword="true"/> if a value is cached for <paramref name="key"/>; otherwise <see langword="false"/>.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key == null)
            {
                value = null;
                return false;
            }

            return _cache.TryGetValue(key, out value);
        }

        /// <summary>
        ///     Removes all cached entries.
        /// </summary>
        public void Clear() => _cache.Clear();
    }
}
