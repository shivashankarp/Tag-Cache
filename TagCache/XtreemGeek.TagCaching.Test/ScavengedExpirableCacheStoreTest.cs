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

namespace XtreemGeek.TagCaching.Test
{
    [TestClass]
    public class ScavengedExpirableCacheStoreTest
    {
        [TestMethod]
        public void TestPriorityBasedScavenging()
        {
            const int MaxCacheItemCount = 1000;
            const decimal ScavengePercentage = 50;
            const int MaxItemsAfterScavenging = (int)(MaxCacheItemCount * ScavengePercentage / 100);
            const int NumberOfItemsToInsert = MaxCacheItemCount + 1;

            ScavengedExpirableCacheStore cacheStore = new ScavengedExpirableCacheStore(new CacheOptions { ExpirationPollInterval = TimeSpan.FromSeconds(1), MaxCacheItemCount = MaxCacheItemCount, ScavengePercentage = ScavengePercentage });

            // insert with increasing priority
            for (int i = 0; i < NumberOfItemsToInsert; i++)
            {
                CacheItem cacheItem = new CacheItem(String.Format("key{0}", i), null, i + 1, null);
                cacheStore.Set(cacheItem);
            }

            Thread.Sleep(3000);

            int cacheItemCount = cacheStore.CacheItemCount;
            Assert.IsTrue(cacheItemCount == MaxItemsAfterScavenging, "scavenging did not occur as expected");

            for (int i = 0; i < cacheItemCount; i++)
            {
                CacheItem cacheItem = cacheStore.Get(String.Format("key{0}", i), false);
                Assert.IsNotNull(cacheItem, "cache item is expected to be present");
            }

            for (int i = cacheItemCount; i < NumberOfItemsToInsert; i++)
            {
                CacheItem cacheItem = cacheStore.Get(String.Format("key{0}", i), false);
                Assert.IsNull(cacheItem, "cache item is expected to be absent");
            }

            // insert with max priority
            for (int i = cacheItemCount; i < NumberOfItemsToInsert; i++)
            {
                CacheItem cacheItem = new CacheItem(String.Format("key{0}", i), null, 0, null);
                cacheStore.Set(cacheItem);
            }

            _ReflectivelyTriggerScavenging(cacheStore);

            cacheItemCount = cacheStore.CacheItemCount;
            Assert.IsTrue(cacheItemCount == MaxItemsAfterScavenging, "scavenging did not occur as expected");

            int numMissing = 0;
            for (int i = NumberOfItemsToInsert - 1; i >= NumberOfItemsToInsert - cacheItemCount; i--)
            {
                CacheItem cacheItem = cacheStore.Get(String.Format("key{0}", i), false);
                numMissing += (cacheItem != null ? 0 : 1);
                Assert.IsTrue(numMissing <= 1, "more than expected number of items missing");
            }


            int numFound = 0;
            for (int i = NumberOfItemsToInsert - cacheItemCount - 1; i >= 0; i--)
            {
                CacheItem cacheItem = cacheStore.Get(String.Format("key{0}", i), false);
                numFound += (cacheItem != null ? 1 : 0);
                Assert.IsTrue(numFound <= 1, "more than expected number of items found");
            }
        }

        [TestMethod]
        public void TestScavengingWithExpiry()
        {
            const int MaxCacheItemCount = 1000;
            const decimal ScavengePercentage = 50;
            const int MaxItemsAfterScavenging = (int)(MaxCacheItemCount * ScavengePercentage / 100);
            const int NumberOfItemsToInsert = MaxCacheItemCount + 1;

            ScavengedExpirableCacheStore cacheStore = new ScavengedExpirableCacheStore(new CacheOptions { ExpirationPollInterval = TimeSpan.FromMinutes(2), MaxCacheItemCount = MaxCacheItemCount, ScavengePercentage = ScavengePercentage });

            TestExpirationSpecifier testExpirationSpecifier = new TestExpirationSpecifier();

            for (int i = 0; i < NumberOfItemsToInsert; i++)
            {
                IExpirationSpecifier[] expirationSpecifiers = i >= MaxItemsAfterScavenging ? new IExpirationSpecifier[] { testExpirationSpecifier } : null;
                CacheItem cacheItem = new CacheItem(String.Format("key{0}", i), null, 0, expirationSpecifiers);
                cacheStore.Set(cacheItem);
            }

            testExpirationSpecifier.Expired = true;
            Thread.Sleep(3000);

            int cacheItemCount = cacheStore.CacheItemCount;
            Assert.IsTrue(cacheItemCount == MaxItemsAfterScavenging, "scavenging improper");

            for (int i = 0; i < cacheItemCount; i++)
            {
                CacheItem cacheItem = cacheStore.Get(String.Format("key{0}", i), false);
                Assert.IsNotNull(cacheItem, "cacheItem expected");
            }
        }

        [TestMethod]
        public void TestScavengingWithHugeNumberOfItems()
        {
            const int MaxCacheItemCount = 1000;
            const decimal ScavengePercentage = 50;

            ScavengedExpirableCacheStore cacheStore = new ScavengedExpirableCacheStore(new CacheOptions { ExpirationPollInterval = TimeSpan.FromMinutes(2), MaxCacheItemCount = MaxCacheItemCount, ScavengePercentage = ScavengePercentage });

            Parallel.For(0, 10, delegate(int ndx)
            {
                for (int i = 0; i < 10000; i++)
                {
                    cacheStore.Set(new CacheItem(String.Format("key_{0}_{1}", ndx, i), null, 0, null));
                }
            });

            Thread.Sleep(3000);
            Assert.IsTrue(cacheStore.CacheItemCount < MaxCacheItemCount + 250, "scavenging failed");
        }

        [TestMethod]
        public void TestScavengingOfOldestAccessedItem()
        {
            const int MaxCacheItemCount = 200;
            const decimal ScavengePercentage = 50;

            ScavengedExpirableCacheStore cacheStore = new ScavengedExpirableCacheStore(new CacheOptions { ExpirationPollInterval = TimeSpan.FromMinutes(2), MaxCacheItemCount = MaxCacheItemCount, ScavengePercentage = ScavengePercentage });

            for (int i = 0; i < 190; i++)
            {
                cacheStore.Set(new CacheItem(String.Format("key{0}", i), null, 0, null));
            }

            Thread.Sleep(100);

            for (int i = 0; i < 100; i++)
            {
                Assert.IsNotNull(cacheStore.Get(String.Format("key{0}", i), false));
            }

            _ReflectivelyTriggerScavenging(cacheStore);

            for (int i = 0; i < 100; i++)
            {
                Assert.IsNotNull(cacheStore.Get(String.Format("key{0}", i), false));
            }

            for (int i = 100; i < 190; i++)
            {
                Assert.IsNull(cacheStore.Get(String.Format("key{0}", i), false));
            }
        }

        [TestMethod]
        public void NoTagMultiThreadedBombardTest()
        {
            CacheOptions cacheOptions = new CacheOptions();
            ScavengedExpirableCacheStore cacheStore = new ScavengedExpirableCacheStore(cacheOptions);

            Parallel.For(0, 5, (i) => _CacheBombard(cacheStore, new Random(i)));

            Assert.IsTrue(cacheStore.CacheItemCount < cacheOptions.MaxCacheItemCount + 250, "scavenging failed");
        }

        private void _CacheBombard(ScavengedExpirableCacheStore cacheStore, Random random)
        {
            object data = new object();

            try
            {
                for (int i = 0; i < 500000; i++)
                {
                    double dice = random.NextDouble();

                    if (dice <= 0.3)
                    {
                        cacheStore.Set(new CacheItem(TagCacheTestUtil.GetRandomKey(random), data, 0, null));
                    }
                    else if (dice <= 0.8)
                    {
                        cacheStore.Get(TagCacheTestUtil.GetRandomKey(random), false);
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

        private void _ReflectivelyTriggerScavenging(ScavengedExpirableCacheStore cacheStore)
        {
            cacheStore.GetType().GetMethod("_ScavengeCacheItems", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(cacheStore, new object[] { null });
        }
    }
}
