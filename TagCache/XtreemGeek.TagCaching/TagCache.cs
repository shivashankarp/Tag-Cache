using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XtreemGeek.TagCaching.Util;
using System.Collections.Concurrent;
using XtreemGeek.TagCaching.Implementation;
using XtreemGeek.TagCaching.Implementation.Tagging;
using XtreemGeek.TagCaching.Implementation.Expiration;

namespace XtreemGeek.TagCaching
{
    /// <summary>
    /// TagCache: a full-fledged, high-performance cache that allows tagging cache items
    /// and later invalidating groups of items based on tag-expressions.
    /// </summary>
    public class TagCache
    {
        public const int HIGHEST_PRIORITY = 0;
        public const int DEFAULT_PRIORITY = 500;
        public const int LOWEST_PRIORITY = 1000;

        private TaggedScavengedExpirableCacheStore _cacheStore;

        public TagCache(int maxCacheItemCount = 25000, decimal scavengePercentage = 20M)
        {
            _cacheStore = new TaggedScavengedExpirableCacheStore(new CacheOptions
            {
                MaxCacheItemCount = maxCacheItemCount,
                ScavengePercentage = scavengePercentage
            });
        }

        #region in-built expiration specifiers

        /// <summary>
        /// Creates IExpirationSpecifier for absolute expiry
        /// </summary>
        /// <param name="utcAbsoluteExpiry"></param>
        /// <returns></returns>
        public IExpirationSpecifier CreateAbsoluteTimeExpirationSpecifier(DateTime utcAbsoluteExpiry)
        {
            return new AbsoluteTimeExpirationSpecifier(utcAbsoluteExpiry);
        }

        /// <summary>
        /// Creates IExpirationSpecifier for sliding window expiry
        /// </summary>
        /// <param name="windowSpan"></param>
        /// <returns></returns>
        public IExpirationSpecifier CreateSlidingWindowExpirationSpecifier(TimeSpan windowSpan)
        {
            return new SlidingWindowExpirationSpecifier(windowSpan);
        }

        /// <summary>
        /// Creates IExpirationSpecifier for tag list
        /// </summary>
        /// <param name="tagList"></param>
        /// <returns></returns>
        public IExpirationSpecifier CreateTagExpirationSpecifier(List<string> tagList)
        {
            return _cacheStore.GetTagExpirationSpecifier(tagList);
        }

        #endregion

        #region overloaded Set APIs

        /// <summary>
        /// To set an item in cache
        /// </summary>
        /// <param name="key">cache item key</param>
        /// <param name="value">cache value</param>
        /// <param name="priority">optional priority</param>
        public void Set(string key, object value, int priority = DEFAULT_PRIORITY)
        {
            this.Set(key, value, priority, null);
        }

        /// <summary>
        /// To set an item in cache
        /// </summary>
        /// <param name="key">cache item key</param>
        /// <param name="value">cache value</param>
        /// <param name="utcAbsoluteExpiry">absolute expiry time in UTC</param>
        /// <param name="priority">optional priority</param>
        public void Set(string key, object value, DateTime utcAbsoluteExpiry, int priority = DEFAULT_PRIORITY)
        {
            this.Set(key, value, priority, new IExpirationSpecifier[] { CreateAbsoluteTimeExpirationSpecifier(utcAbsoluteExpiry) });
        }

        /// <summary>
        /// To set an item in cache
        /// </summary>
        /// <param name="key">cache item key</param>
        /// <param name="value">cache value</param>
        /// <param name="slidingWindowSpan">sliding window TimeSpan</param>
        /// <param name="priority">optional priority</param>
        public void Set(string key, object value, TimeSpan slidingWindowSpan, int priority = DEFAULT_PRIORITY)
        {
            this.Set(key, value, priority, new IExpirationSpecifier[] { CreateSlidingWindowExpirationSpecifier(slidingWindowSpan) });
        }

        /// <summary>
        /// To set an item in cache
        /// </summary>
        /// <param name="key">cache item key</param>
        /// <param name="value">cache value</param>
        /// <param name="tagList">tags (aka labels) to be associated with the cache value</param>
        /// <param name="priority">optional priority</param>
        public void Set(string key, object value, List<string> tagList, int priority = DEFAULT_PRIORITY)
        {
            this.Set(key, value, priority, new IExpirationSpecifier[] { CreateTagExpirationSpecifier(tagList) });
        }

        /// <summary>
        /// To set an item in cache
        /// </summary>
        /// <param name="key">cache item key</param>
        /// <param name="value">cache value</param>
        /// <param name="tagList">tags (aka labels) to be associated with the cache value</param>
        /// <param name="utcAbsoluteExpiry">absolute expiry time in UTC</param>
        /// <param name="priority">optional priority</param>
        public void Set(string key, object value, List<string> tagList, DateTime utcAbsoluteExpiry, int priority = DEFAULT_PRIORITY)
        {
            this.Set(key, value, priority, new IExpirationSpecifier[] { CreateAbsoluteTimeExpirationSpecifier(utcAbsoluteExpiry), CreateTagExpirationSpecifier(tagList) });
        }

        /// <summary>
        /// To set an item in cache
        /// </summary>
        /// <param name="key">cache item key</param>
        /// <param name="value">cache value</param>
        /// <param name="tagList">tags (aka labels) to be associated with the cache value</param>
        /// <param name="slidingWindowSpan">sliding window TimeSpan</param>
        /// <param name="priority">optional priority</param>
        public void Set(string key, object value, List<string> tagList, TimeSpan slidingWindowSpan, int priority = DEFAULT_PRIORITY)
        {
            this.Set(key, value, priority, new IExpirationSpecifier[] { CreateSlidingWindowExpirationSpecifier(slidingWindowSpan), CreateTagExpirationSpecifier(tagList) });
        }

        /// <summary>
        /// To set an item in cache
        /// </summary>
        /// <param name="key">cache item key</param>
        /// <param name="value">cache value</param>
        /// <param name="priority">priority</param>
        /// <param name="expirationSpecifierList">list of expiration specifiers</param>
        public void Set(string key, object value, int priority, IExpirationSpecifier[] expirationSpecifierList)
        {
            Guard.Assert(key != null, "Cache key cannot be null");
            Guard.Assert(value != null, "Cache value cannot be null");
            Guard.Assert(priority >= HIGHEST_PRIORITY && priority <= LOWEST_PRIORITY, "Invalid priority {0}", priority);

            _cacheStore.Set(new CacheItem(key, value, priority, expirationSpecifierList));
        }

        #endregion

        /// <summary>
        /// to get a value from cache
        /// </summary>
        /// <param name="key">cache item key</param>
        /// <returns></returns>
        public object Get(string key)
        {
            Guard.Assert(key != null, "Cache key cannot be null");

            CacheItem cacheItem = _cacheStore.Get(key);
            return cacheItem != null ? cacheItem.Value : null;
        }

        /// <summary>
        /// to remove an item from cache
        /// </summary>
        /// <param name="key">cache item key</param>
        public void Remove(string key)
        {
            Guard.Assert(key != null, "Cache key cannot be null");

            _cacheStore.Remove(key);
        }

        /// <summary>
        /// to invalidate item(s) from cache based on tag expression
        /// </summary>
        /// <param name="tagExpression">This is used similar to sum-of-product expression. The outer list signifies OR and the inner list signifies AND.</param>
        public void Invalidate(List<List<string>> tagExpression)
        {
            Guard.Assert(tagExpression != null, "TagExpression cannot be null");

            _cacheStore.Invalidate(tagExpression);
        }

        /// <summary>
        /// to invalidate item(s) from cache based on tag list
        /// </summary>
        /// <param name="tagExpression">This spcecifies the tag combination that needs to be invalided</param>
        public void Invalidate(List<string> tagList)
        {
            Guard.Assert(tagList != null, "TagExpression cannot be null");

            _cacheStore.Invalidate(new List<List<string>> { tagList });
        }

        /// <summary>
        /// to invalidate item(s) from cache that match the tag
        /// </summary>
        /// <param name="tag">This specifies the tag the needs to be invalided.</param>
        public void Invalidate(string tag)
        {
            Guard.Assert(tag != null, "TagExpression cannot be null");

            _cacheStore.Invalidate(new List<List<string>> { new List<string> { tag } });
        }
    }
}
