using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using XtreemGeek.TagCaching.Util;

namespace XtreemGeek.TagCaching.Implementation.Storage
{
    /// <summary>
    /// The simplest key-value store that takes advantage of .Net concurrency library
    /// to do thread safe operations without much lock overhead
    /// </summary>
    internal class CacheStore
    {
        private CacheOptions _CacheOptions { get; set; }

        private ConcurrentDictionary<string, CacheItem> _CacheItemMap { get; set; }

        public CacheStore(CacheOptions cacheOptions)
        {
            _CacheOptions = cacheOptions;
            _CacheItemMap = new ConcurrentDictionary<string, CacheItem>();
        }

        public void Set(CacheItem cacheItem)
        {
            _CacheItemMap[cacheItem.Key] = cacheItem;
        }

        public CacheItem Get(string key)
        {
            CacheItem cacheItem;
            _CacheItemMap.TryGetValue(key, out cacheItem);
            return cacheItem;
        }

        public void Remove(string key)
        {
            CacheItem cacheItem;
            _CacheItemMap.TryRemove(key, out cacheItem);
        }

        /// <summary>
        /// WARNING: This is a costly operation; internally it locks all buckets to create the list
        /// </summary>
        public ICollection<string> GetKeys()
        {
            return _CacheItemMap.Keys;
        }

        /// <summary>
        /// WARNING: This is a costly operation; internally it locks all buckets to count
        /// </summary>
        public int CacheItemCount
        {
            get { return _CacheItemMap.Count; }
        }
    }
}
