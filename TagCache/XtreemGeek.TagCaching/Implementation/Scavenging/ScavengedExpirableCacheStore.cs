using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XtreemGeek.TagCaching.Implementation.Expiration;
using XtreemGeek.TagCaching.Util;
using System.Threading;

namespace XtreemGeek.TagCaching.Implementation.Scavenging
{
    /// <summary>
    /// Cache Store that can scavenge cache items when the maximum allowed limit is reached/exceeded
    /// This is built upon an expirable cache store
    /// </summary>
    internal class ScavengedExpirableCacheStore
    {
        private CacheOptions _CacheOptions { get; set; }

        private ExpirableCacheStore _ExpirableCacheStore { get; set; }

        private int _setCount;
        private int _scavengeScheduled;

        public ScavengedExpirableCacheStore(CacheOptions cacheOptions)
        {
            Guard.Assert(cacheOptions.MaxCacheItemCount > 0, "invalid cache item count");
            Guard.Assert(cacheOptions.ScavengePercentage > 0, "invalid scavenge percentage");

            _CacheOptions = cacheOptions;
            _ExpirableCacheStore = new ExpirableCacheStore(_CacheOptions);
        }

        public void Set(CacheItem cacheItem)
        {
            cacheItem.LastAccessedTimeUtc = DateTime.UtcNow;
            _ExpirableCacheStore.Set(cacheItem);

            // we check for scavenge threshold exceedence only every N sets in order to avoid the costly call to ExpirableCacheStore.CacheItemCount
            if (Interlocked.Increment(ref _setCount) % 200 == 0)
            {
                if (_IsScavengingRequired())
                {
                    if (Interlocked.CompareExchange(ref _scavengeScheduled, 1, 0) == 0)
                    {
                        ThreadPool.QueueUserWorkItem(_ScavengeCacheItems);
                    }
                }

                _setCount = 0;
                Thread.MemoryBarrier();
            }
        }

        // WARNING:  This is a costly call since the CacheItemCount calculation is costly (acquires locks)
        private bool _IsScavengingRequired()
        {
            int cacheItemCount = _ExpirableCacheStore.CacheItemCount;
            return cacheItemCount >= _CacheOptions.MaxCacheItemCount;
        }

        private void _ScavengeCacheItems(object state)
        {
            // we repeat scavenging process in an iterative code block
            // this is in order to handle scenarios where even before the scavenging completes
            // the cache is flooded with more objects that make it exceed the allowed limit
            try
            {
                int scavengeIterationCount = 0;
                do
                {
                    ++scavengeIterationCount;

                    // we get the key list and query each cache item, this will ensure that expired items get removed
                    ICollection<string> keyList = _ExpirableCacheStore.GetKeys();
                    List<CacheItem> cacheItemList = new List<CacheItem>(keyList.Count);

                    foreach (string key in keyList)
                    {
                        CacheItem cacheItem = _ExpirableCacheStore.Get(key, true);
                        if (cacheItem != null)
                        {
                            cacheItemList.Add(cacheItem);
                        }
                    }

                    int targetNumberOfItemsInCache = (int)Math.Ceiling((_CacheOptions.MaxCacheItemCount * (100 - _CacheOptions.ScavengePercentage) / 100));
                    int numberOfItemsToRemove = cacheItemList.Count - targetNumberOfItemsInCache;

                    if (numberOfItemsToRemove > 0)
                    {
                        // a priority based LRU sort
                        cacheItemList.Sort(delegate(CacheItem a, CacheItem b)
                        {
                            if (a.Priority == b.Priority)
                            {
                                if (a.LastAccessedTimeUtc == b.LastAccessedTimeUtc)
                                {
                                    return 0;
                                }

                                return a.LastAccessedTimeUtc > b.LastAccessedTimeUtc ? 1 : -1;
                            }
                            return b.Priority - a.Priority;
                        });

                        for (int i = 0, j = 0; i < cacheItemList.Count && j < numberOfItemsToRemove; i++)
                        {
                            CacheItem cacheItem = cacheItemList[i];
                            CacheItem cacheItemTemp = _ExpirableCacheStore.Get(cacheItem.Key, true);

                            bool scavenged = false;
                            if (cacheItemTemp == null)
                            {
                                // item got removed or expired
                                scavenged = true;
                            }
                            else if (Object.ReferenceEquals(cacheItem, cacheItemTemp))
                            {
                                // we check for Object.ReferenceEquals since the item to be scanvenged
                                //  could have been replaced with a new item
                                _ExpirableCacheStore.Remove(cacheItem.Key);
                                scavenged = true;
                            }

                            if (scavenged)
                            {
                                ++j;
                            }
                        }
                    }
                }
                while (scavengeIterationCount < 10 && _IsScavengingRequired());
            }
            catch
            {
            }
            finally
            {
                _scavengeScheduled = 0;
                Thread.MemoryBarrier();
            }
        }

        /// <summary>
        /// item Get
        /// </summary>
        /// <param name="key"></param>
        /// <param name="forBookKeeping">signifies whether the audit has to be carried out or not</param>
        /// <returns></returns>
        public CacheItem Get(string key, bool forBookKeeping)
        {
            CacheItem cacheItem = _ExpirableCacheStore.Get(key, forBookKeeping);
            
            if (cacheItem != null && !forBookKeeping)
            {
                cacheItem.LastAccessedTimeUtc = DateTime.UtcNow;
            }

            return cacheItem;
        }

        public void Remove(string key)
        {
            _ExpirableCacheStore.Remove(key);
        }

        /// <summary>
        /// WARNING: this is a costly call, use sparsely
        /// </summary>
        /// <returns></returns>
        public ICollection<string> GetKeys()
        {
            return _ExpirableCacheStore.GetKeys();
        }

        /// <summary>
        /// WARNING: this is a costly call, use sparsely
        /// </summary>
        /// <returns></returns>
        public int CacheItemCount
        {
            get { return _ExpirableCacheStore.CacheItemCount; }
        }
    }
}
