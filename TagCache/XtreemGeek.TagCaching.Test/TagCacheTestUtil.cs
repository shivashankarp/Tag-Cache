using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XtreemGeek.TagCaching.Implementation.Tagging;

namespace XtreemGeek.TagCaching.Test
{
    internal static class TagCacheTestUtil
    {
        public const int MAX_TAG = 200;

        public const int MAX_KEY = 20000;

        public static string GetTag(int i)
        {
            return String.Format("Tag-{0:D3}", i);
        }

        public static string GetKey(int i)
        {
            return String.Format("Key-{0:D5}", i);
        }

        public static string GetRandomKey(Random random)
        {
            return GetKey(random.Next(MAX_KEY));
        }

        public static string GetRandomTag(Random random, int maxTag)
        {
            return GetTag(random.Next(maxTag));
        }

        public static List<string> GetRandomTagList(Random random, int numTags, int maxTag)
        {
            HashSet<string> tagSet = new HashSet<string>();
            while (tagSet.Count < numTags)
            {
                tagSet.Add(GetRandomTag(random, maxTag));
            }
            return new List<string>(tagSet);
        }

        public static IExpirationSpecifier[] GetTagExpirationSpecifier(TaggedScavengedExpirableCacheStore cacheStore, List<string> tagList)
        {
            return new IExpirationSpecifier[] { cacheStore.GetTagExpirationSpecifier(tagList) };
        }

        public static IExpirationSpecifier[] GetRandomTagsExpirationSpecifier(TaggedScavengedExpirableCacheStore cacheStore, Random random, int numTags, int maxTag)
        {
            List<string> tagList = GetRandomTagList(random, numTags, maxTag);
            return GetTagExpirationSpecifier(cacheStore, tagList);
        }

        public static string GetRandomTag(Random random)
        {
            return GetRandomTag(random, MAX_TAG);
        }

        public static List<string> GetRandomTagList(Random random, int numTags)
        {
            return GetRandomTagList(random, numTags, MAX_TAG);
        }
    }
}
