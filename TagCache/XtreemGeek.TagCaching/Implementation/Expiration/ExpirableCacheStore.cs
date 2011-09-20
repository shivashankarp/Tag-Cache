using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XtreemGeek.TagCaching.Implementation.Storage;
using System.Threading;
using XtreemGeek.TagCaching.Util;

namespace XtreemGeek.TagCaching.Implementation.Expiration
{
    /// <summary>
    /// A cache store that knows to takes care of value expiry
    /// This is built upon a simple cache store that supports a scalable dictionary like functionality
    /// </summary>
    internal class ExpirableCacheStore
    {
        private CacheStore _CacheStore { get; set; }

        private CacheOptions _CacheOptions { get; set; }

        private IntervalTimer _IntervalTimer { get; set; }

        public ExpirableCacheStore(CacheOptions cacheOptions)
        {
            _CacheOptions = cacheOptions;
            _CacheStore = new CacheStore(_CacheOptions);

            Guard.Assert(cacheOptions.ExpirationPollInterval.TotalSeconds >= 1.0, "expiration poll time interval must be at least one second long");
            _IntervalTimer = new IntervalTimer(cacheOptions.ExpirationPollInterval);
            _IntervalTimer.OnInterval += new Action(_IntervalTimer_OnInterval);
        }

        /// <summary>
        /// Just iterating through all the keys once in a while
        /// and doing a Get() should take care of removing expired items.
        /// Note that we're explicitly indicating the Get() call in this
        /// method as 'for-bookkeeping' so as to not interfere with
        /// any audits that may be happening based on external Get()
        /// </summary>
        private void _IntervalTimer_OnInterval()
        {
            foreach (string key in _CacheStore.GetKeys())
            {
                Get(key, true);
            }
        }

        private bool _IsExpired(CacheItem cacheItem)
        {
            return cacheItem.ExpirationSpecifierList != null
                ? Array.FindIndex(cacheItem.ExpirationSpecifierList, (es) => es.Expired) > -1
                : false;
        }

        private void _RaiseOnCacheItemAccessed(CacheItem cacheItem)
        {
            if (cacheItem.ExpirationSpecifierList != null)
            {
                Array.ForEach(cacheItem.ExpirationSpecifierList, (es) => es.OnCacheItemAccessed());
            }
        }

        public void Set(CacheItem cacheItem)
        {
            // make sure the OnCacheItemAccessed even is raised prior to checking for expiry or adding item to cache
            // this is done so that the expiration verifiers set their internal properties correctly before being used for expiry checks
            _RaiseOnCacheItemAccessed(cacheItem);   

            if (!_IsExpired(cacheItem))
            {
                _CacheStore.Set(cacheItem);
            }
        }

        public CacheItem Get(string key, bool forBookKeeping)
        {
            CacheItem cacheItem = _CacheStore.Get(key);

            // remove cache item on expiry
            if (cacheItem != null && _IsExpired(cacheItem))
            {
                _CacheStore.Remove(key);
                cacheItem = null;
            }

            // raise audit call only if this call is not for bookkeeping
            if (cacheItem != null && !forBookKeeping)
            {
                _RaiseOnCacheItemAccessed(cacheItem);
            }

            return cacheItem;
        }

        public void Remove(string key)
        {
            _CacheStore.Remove(key);
        }

        /// <summary>
        /// WARNING: this is a costly call, use sparsely
        /// </summary>
        /// <returns></returns>
        public ICollection<string> GetKeys()
        {
            return _CacheStore.GetKeys();
        }

        /// <summary>
        /// WARNING: this is a costly call, use sparsely
        /// </summary>
        /// <returns></returns>
        public int CacheItemCount
        {
            get { return _CacheStore.CacheItemCount; }
        }
    }
}
