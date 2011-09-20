using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XtreemGeek.TagCaching.Implementation;
using XtreemGeek.TagCaching.Implementation.Expiration;
using System.Threading;

namespace XtreemGeek.TagCaching.Test
{
    [TestClass]
    public class ExpirableCacheStoreTest
    {
        [TestMethod]
        public void TestCustomExpiration()
        {
            ExpirableCacheStore expirableCacheStore = new ExpirableCacheStore(new CacheOptions());

            CacheItem cacheItem = new CacheItem("cacheItem", null, 0, new IExpirationSpecifier[] { new TestExpirationSpecifier() });
            expirableCacheStore.Set(cacheItem);

            Assert.IsTrue(Object.ReferenceEquals(cacheItem, expirableCacheStore.Get(cacheItem.Key, false)), "unable to get object");

            ((TestExpirationSpecifier)cacheItem.ExpirationSpecifierList[0]).Expired = true;
            Assert.IsNull(expirableCacheStore.Get(cacheItem.Key, false), "unable to expire object");
        }

        [TestMethod]
        public void TestAbsoluteExpiration()
        {
            ExpirableCacheStore expirableCacheStore = new ExpirableCacheStore(new CacheOptions());

            CacheItem cacheItem = new CacheItem("cacheItem", null, 0, new IExpirationSpecifier[] { new AbsoluteTimeExpirationSpecifier(DateTime.UtcNow.AddSeconds(1)) });
            expirableCacheStore.Set(cacheItem);

            Assert.IsTrue(Object.ReferenceEquals(cacheItem, expirableCacheStore.Get(cacheItem.Key, false)), "unable to get object");
            Thread.Sleep(300);
            Assert.IsTrue(Object.ReferenceEquals(cacheItem, expirableCacheStore.Get(cacheItem.Key, false)), "unable to get object");
            Thread.Sleep(900);
            Assert.IsNull(expirableCacheStore.Get(cacheItem.Key, false), "unable to expire object");
        }

        [TestMethod]
        public void TestSlidingExpiration()
        {
            ExpirableCacheStore expirableCacheStore = new ExpirableCacheStore(new CacheOptions());

            CacheItem cacheItem = new CacheItem("cacheItem", null, 0, new IExpirationSpecifier[] { new SlidingWindowExpirationSpecifier(TimeSpan.FromSeconds(1)) });
            expirableCacheStore.Set(cacheItem);

            Assert.IsTrue(Object.ReferenceEquals(cacheItem, expirableCacheStore.Get(cacheItem.Key, false)), "unable to get object");
            Thread.Sleep(300);
            Assert.IsTrue(Object.ReferenceEquals(cacheItem, expirableCacheStore.Get(cacheItem.Key, false)), "unable to get object");
            Thread.Sleep(900);
            Assert.IsTrue(Object.ReferenceEquals(cacheItem, expirableCacheStore.Get(cacheItem.Key, false)), "unable to get object");
            Thread.Sleep(1100);
            Assert.IsNull(expirableCacheStore.Get(cacheItem.Key, false), "unable to expire object");
        }

        [TestMethod]
        public void TestExpirationPolling()
        {
            ExpirableCacheStore expirableCacheStore = new ExpirableCacheStore(new CacheOptions { ExpirationPollInterval = TimeSpan.FromSeconds(1) });

            CacheItem cacheItem = new CacheItem("cacheItem", null, 0, new IExpirationSpecifier[] { new TestExpirationSpecifier() });
            expirableCacheStore.Set(cacheItem);

            Assert.AreEqual(expirableCacheStore.CacheItemCount, 1, "count mismatch");
            
            Thread.Sleep(1100);
            Assert.AreEqual(expirableCacheStore.CacheItemCount, 1, "count mismatch");

            ((TestExpirationSpecifier)cacheItem.ExpirationSpecifierList[0]).Expired = true;
            Thread.Sleep(1100);
            Assert.AreEqual(expirableCacheStore.CacheItemCount, 0, "count mismatch");
        }
    }
}
