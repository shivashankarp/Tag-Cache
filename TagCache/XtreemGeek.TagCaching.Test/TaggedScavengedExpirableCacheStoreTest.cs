using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XtreemGeek.TagCaching.Implementation.Scavenging;
using XtreemGeek.TagCaching.Implementation;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using XtreemGeek.TagCaching.Implementation.Tagging;
using System.Collections.Concurrent;

namespace XtreemGeek.TagCaching.Test
{
    [TestClass]
    public class TaggedScavengedExpirableCacheStoreTest
    {
        [TestMethod]
        public void TestMultipleTagInvalidation()
        {
            //10C4 + 10C3 + 10C2 + 10C1 = 385 combinations
            const int MAX_TAG = 10;
            const int MAX_TAG_LIST_LENGTH = 4;

            TaggedScavengedExpirableCacheStore cacheStore = new TaggedScavengedExpirableCacheStore(new CacheOptions());

            Random random = new Random();

            for (int i = 0; i < 500; i++)
            {
                for (int j = 0; j < 1000; j++)
                {
                    cacheStore.Set(new CacheItem(j.ToString(), null, 0, TagCacheTestUtil.GetRandomTagsExpirationSpecifier(cacheStore, random, random.Next(1, MAX_TAG_LIST_LENGTH), MAX_TAG)));
                }

                for (int j = 0; j < 5; j++)
                {
                    List<string> tagListToInvalidate = TagCacheTestUtil.GetRandomTagList(random, random.Next(1, MAX_TAG_LIST_LENGTH), MAX_TAG);
                    cacheStore.Invalidate(new List<List<string>> { tagListToInvalidate });
                    _EnsureCacheItemsDontMatch(cacheStore, tagListToInvalidate);
                }
            }
        }

        [TestMethod]
        public void TestTagInfoGarbageCollection()
        {
            TaggedScavengedExpirableCacheStore cacheStore = new TaggedScavengedExpirableCacheStore(new CacheOptions());
            Random random = new Random();

            HashSet<string> usedTagSet = new HashSet<string>();

            for (int i = 0; i < 200000; i++)
            {
                string key = TagCacheTestUtil.GetRandomKey(random);
                List<string> tagList = TagCacheTestUtil.GetRandomTagList(random, random.Next(1, 1));

                cacheStore.Set(new CacheItem(key, null, 0, TagCacheTestUtil.GetTagExpirationSpecifier(cacheStore, tagList)));
                usedTagSet.UnionWith(tagList);
            }

            HashSet<string> invalidatedTagSet = new HashSet<string>();

            for (int i = 0; i < TagCacheTestUtil.MAX_TAG * 3 / 4; i++)
            {
                string tag = TagCacheTestUtil.GetRandomTag(random);
                cacheStore.Invalidate(new List<List<string>> { new List<string> { tag } });
                invalidatedTagSet.Add(tag);
            }

            usedTagSet.ExceptWith(invalidatedTagSet);

            List<string> validTagList = new List<string>(usedTagSet);
            _EnsureTags(cacheStore, validTagList);
        }

        [TestMethod]
        public void MultipleTagInvalidationTest()
        {
            const int MAX_TAG = 10;
            const int MAX_TAG_LIST_LENGTH = 4;

            Random random = new Random();

            for (int iter = 0; iter < 2; iter++)
            {
                TaggedScavengedExpirableCacheStore cacheStore = new TaggedScavengedExpirableCacheStore(new CacheOptions());
                object data = new object();

                for (int i = 0; i < 100000; i++)
                {
                    cacheStore.Set(new CacheItem(Convert.ToString(i), data, 0, TagCacheTestUtil.GetRandomTagsExpirationSpecifier(cacheStore, random, random.Next(1, MAX_TAG_LIST_LENGTH), MAX_TAG)));
                }

                Thread.Sleep(1000);

                for (int j = 0; j < 1000; j++)
                {
                    List<string> tagListToInvalidate = TagCacheTestUtil.GetRandomTagList(random, random.Next(1, MAX_TAG_LIST_LENGTH), MAX_TAG);
                    cacheStore.Invalidate(new List<List<string>> { tagListToInvalidate });
                    _EnsureCacheItemsDontMatch(cacheStore, tagListToInvalidate);
                }
            }
        }

        [TestMethod]
        public void SingleTagMultiThreadedBombardTest()
        {
            TaggedScavengedExpirableCacheStore cacheStore = new TaggedScavengedExpirableCacheStore(new CacheOptions());

            Parallel.For(0, 5, (i) => _CacheBombard(cacheStore, new Random(i)));

            // invalidate a few tags in order to ensure purge queue has some value
            Random random = new Random();
            for (int i = 0; i < TagCacheTestUtil.MAX_TAG / 3; i++)
            {
                cacheStore.Invalidate(new List<List<string>> { new List<string> { TagCacheTestUtil.GetRandomTag(random) } });
            }

            _ValidateCacheConsistency(cacheStore);
        }

        private void _CacheBombard(TaggedScavengedExpirableCacheStore cacheStore, Random random)
        {
            object data = new object();

            try
            {
                for (int i = 0; i < 100000; i++)
                {
                    double dice = random.NextDouble();

                    if (dice <= 0.3)
                    {
                        cacheStore.Set(new CacheItem(TagCacheTestUtil.GetRandomKey(random), data, 0, TagCacheTestUtil.GetTagExpirationSpecifier(cacheStore, TagCacheTestUtil.GetRandomTagList(random, random.Next(1, 4)))));
                    }
                    else if (dice <= 0.8)
                    {
                        cacheStore.Get(TagCacheTestUtil.GetRandomKey(random));
                    }
                    else if (dice <= 0.9)
                    {
                        cacheStore.Invalidate(new List<List<string>> { new List<string> { TagCacheTestUtil.GetRandomTag(random) } });
                    }
                    else
                    {
                        cacheStore.Remove(TagCacheTestUtil.GetRandomKey(random));
                    }
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
            }
        }

        private void _EnsureTags(TaggedScavengedExpirableCacheStore cacheStore, List<string> validTags)
        {
            _TouchAllCacheItems(cacheStore);
            _WaitAndGC();

            TagInfoMap tagInfoMap = (TagInfoMap)cacheStore.GetType().GetProperty("_TagInfoMap", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(cacheStore, null);
            tagInfoMap.GetType().GetMethod("_ScavengeTagMap", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(tagInfoMap, new object[]{});

            ConcurrentDictionary<string, WeakReference> tagInfoByTag = (ConcurrentDictionary<string, WeakReference>)tagInfoMap.GetType().GetProperty("_TagInfoByTag", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tagInfoMap, null);
            ICollection<string> tagList = tagInfoByTag.Keys;

            int numberOfTagDiscrepencies = 0;
            foreach (string tag in tagList)
            {
                if (!validTags.Contains(tag))
                {
                    numberOfTagDiscrepencies++;
                    _EnsureCacheItemsDontMatch(cacheStore, new List<string> { tag });
                }
            }

            Assert.IsTrue(numberOfTagDiscrepencies == 0, "tag GC failed");
            validTags.ForEach(t => Assert.IsTrue(tagList.Contains(t)));
        }

        private void _TouchAllCacheItems(TaggedScavengedExpirableCacheStore cacheStore)
        {
            foreach (string key in cacheStore.GetKeys())
            {
                cacheStore.Get(key);
            }
        }

        private void _WaitAndGC()
        {
            Thread.Sleep(1000);
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void _EnsureCacheItemsDontMatch(TaggedScavengedExpirableCacheStore cacheStore, List<string> tagCombinationList)
        {
            HashSet<string> tagCombination = new HashSet<string>(tagCombinationList);

            foreach (string key in cacheStore.GetKeys())
            {
                CacheItem cacheItem = cacheStore.Get(key);
                if (cacheItem != null)
                {
                    IExpirationSpecifier[] expirationSpecifierList = (IExpirationSpecifier[])Array.FindAll(cacheItem.ExpirationSpecifierList, (esp) => esp is TagExpirationSpecifier);
                    foreach (TagExpirationSpecifier expirationSpecifier in expirationSpecifierList)
                    {
                        PropertyInfo propertyInfo = expirationSpecifier.GetType().GetProperty("_TagInfoList", BindingFlags.NonPublic | BindingFlags.Instance);
                        List<TagInfo> tagInfoList = (List<TagInfo>)propertyInfo.GetValue(expirationSpecifier, null);

                        bool subset = true;
                        foreach(string tag in tagCombinationList)
                        {
                            subset = subset && tagInfoList.Exists(t => t.Tag == tag);
                        }

                        Assert.IsFalse(subset);
                    }
                }
            }
        }

        private List<string> _GetTagsReferredInCacheItems(TaggedScavengedExpirableCacheStore cacheStore)
        {
            HashSet<string> tagSet = new HashSet<string>();

            foreach (string key in cacheStore.GetKeys())
            {
                CacheItem cacheItem = cacheStore.Get(key);
                if (cacheItem != null)
                {
                    IExpirationSpecifier[] expirationSpecifierList = (IExpirationSpecifier[])Array.FindAll(cacheItem.ExpirationSpecifierList, (esp) => esp is TagExpirationSpecifier);
                    foreach (TagExpirationSpecifier expirationSpecifier in expirationSpecifierList)
                    {
                        PropertyInfo propertyInfo = expirationSpecifier.GetType().GetProperty("_TagInfoList", BindingFlags.NonPublic | BindingFlags.Instance);
                        List<TagInfo> tagInfoList = (List<TagInfo>)propertyInfo.GetValue(expirationSpecifier, null);
                        tagInfoList.ForEach(ti => tagSet.Add(ti.Tag));
                    }
                }
            }

            return tagSet.ToList();
        }

        private void _EnsureTagExpirationStateValidity(TaggedScavengedExpirableCacheStore cacheStore)
        {
            foreach (string key in cacheStore.GetKeys())
            {
                CacheItem cacheItem = cacheStore.Get(key);
                if (cacheItem != null)
                {
                    IExpirationSpecifier[] expirationSpecifierList = (IExpirationSpecifier[])Array.FindAll(cacheItem.ExpirationSpecifierList, (esp) => esp is TagExpirationSpecifier);
                    foreach (TagExpirationSpecifier expirationSpecifier in expirationSpecifierList)
                    {
                        PropertyInfo tagInfoPropertyInfo = expirationSpecifier.GetType().GetProperty("_TagInfoList", BindingFlags.NonPublic | BindingFlags.Instance);
                        PropertyInfo tagInvalidationPropertyInfo = expirationSpecifier.GetType().GetProperty("_TagInvalidationTokenList", BindingFlags.NonPublic | BindingFlags.Instance);

                        List<TagInfo> tagInfoList = (List<TagInfo>) tagInfoPropertyInfo.GetValue(expirationSpecifier, null);
                        List<object> invalidationTokenList = (List<object>)tagInvalidationPropertyInfo.GetValue(expirationSpecifier, null);

                        for(int i = 0; i < tagInfoList.Count; ++i)
                        {
                            foreach (TagInvalidationInfo invalidationInfo in tagInfoList[i].TagInvalidationInfoList.GetEnumerable(invalidationTokenList[i]))
                            {
                                Assert.Fail("invalidation token is not supposed to be available");
                            }
                        }
                    }
                }
            }
        }

        private void _ValidateCacheConsistency(TaggedScavengedExpirableCacheStore cacheStore)
        {
            _TouchAllCacheItems(cacheStore);
            _WaitAndGC();

            List<string> tagList = _GetTagsReferredInCacheItems(cacheStore);
            _EnsureTags(cacheStore, tagList);
            _EnsureTagExpirationStateValidity(cacheStore);
        }
    }
}
