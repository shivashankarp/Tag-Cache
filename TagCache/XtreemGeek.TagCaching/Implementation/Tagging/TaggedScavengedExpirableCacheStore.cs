using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XtreemGeek.TagCaching.Implementation.Scavenging;

namespace XtreemGeek.TagCaching.Implementation.Tagging
{
    /// <summary>
    /// Cache Store that supports tagging of cached items and invalidating based on tags
    /// This is built upon a scavenged expirable cache store
    /// </summary>
    internal class TaggedScavengedExpirableCacheStore
    {
        private TagInfoMap _TagInfoMap { get; set; }
        private CacheOptions _CacheOptions { get; set; }
        private ScavengedExpirableCacheStore _ScavengedExpirableCacheStore { get; set; }

        public TaggedScavengedExpirableCacheStore(CacheOptions cacheOptions)
        {
            _CacheOptions = cacheOptions;
            _TagInfoMap = new TagInfoMap(_CacheOptions);
            _ScavengedExpirableCacheStore = new ScavengedExpirableCacheStore(_CacheOptions);
        }

        public void Set(CacheItem cacheItem)
        {
            _ScavengedExpirableCacheStore.Set(cacheItem);
        }

        public CacheItem Get(string key)
        {
            return _ScavengedExpirableCacheStore.Get(key, false);
        }

        public void Remove(string key)
        {
            _ScavengedExpirableCacheStore.Remove(key);
        }

        /// <summary>
        /// Invalidate just marks for invalidation and doesn't remove the cache items
        ///   at this point; hence this call is very quick.
        /// To be precise, this call takes O(n), where n is the number of tags in the
        ///   tagExpression.
        /// </summary>
        /// <param name="tagExpression"></param>
        public void Invalidate(List<List<string>> tagExpression)
        {
            _TagInfoMap.SetInvalidateByTagExpression(tagExpression);
        }

        public TagExpirationSpecifier GetTagExpirationSpecifier(List<string> tagList)
        {
            return new TagExpirationSpecifier(_TagInfoMap.GetTagInfoList(tagList));
        }

        /// <summary>
        /// WARNING: this is a costly call, use sparsely
        /// </summary>
        /// <returns></returns>
        public ICollection<string> GetKeys()
        {
            return _ScavengedExpirableCacheStore.GetKeys();
        }
    }
}
