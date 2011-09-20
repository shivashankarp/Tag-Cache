using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XtreemGeek.TagCaching.Implementation;
using XtreemGeek.TagCaching.Implementation.Storage;
using System.Threading.Tasks;

namespace XtreemGeek.TagCaching.Test
{
    [TestClass]
    public class CacheStoreTest
    {
        [TestMethod]
        public void TestSetGetRemove()
        {
            CacheStore cacheStore = new CacheStore(new CacheOptions());

            CacheItem cacheItem1 = new CacheItem("cacheItem1", null, 0, null);
            CacheItem cacheItem2 = new CacheItem("cacheItem2", null, 0, null);
            
            cacheStore.Set(cacheItem1);
            cacheStore.Set(cacheItem2);
            
            Assert.IsTrue(Object.ReferenceEquals(cacheItem1, cacheStore.Get("cacheItem1")), "unable to read back value from ICacheStore");
            Assert.IsTrue(Object.ReferenceEquals(cacheItem2, cacheStore.Get("cacheItem2")), "unable to read back value from ICacheStore");

            cacheStore.Remove(cacheItem1.Key);
            Assert.IsNull(cacheStore.Get(cacheItem1.Key), "failed to remove cache item");
            Assert.IsTrue(Object.ReferenceEquals(cacheItem2, cacheStore.Get(cacheItem2.Key)), "unable to read back value from ICacheStore");

            cacheStore.Remove(cacheItem2.Key);
            Assert.IsNull(cacheStore.Get(cacheItem2.Key), "failed to remove cache item");
        }

        [TestMethod]
        public void TestSetGetMultiThreaded()
        {
            CacheStore cacheStore = new CacheStore(new CacheOptions());

            ParallelLoopResult result = Parallel.For(1, 100000, delegate(int ndx)
            {
                string key = String.Format("cacheItem{0}", ndx);
                CacheItem cacheItem = new CacheItem(key, null, 0, null);
                cacheStore.Set(cacheItem);
                Assert.IsTrue(Object.ReferenceEquals(cacheItem, cacheStore.Get(cacheItem.Key)), "unable to read back value from ICacheStore");
            });

            Assert.IsTrue(result.IsCompleted, "failed multi threaded Set/Get");
        }

        [TestMethod]
        public void TestGetKeys()
        {
            CacheStore cacheStore = new CacheStore(new CacheOptions());

            CacheItem cacheItem1 = new CacheItem("cacheItem1", "cacheItem1", 0, null);
            CacheItem cacheItem2 = new CacheItem("cacheItem2", "cacheItem2", 0, null);

            cacheStore.Set(cacheItem1);
            cacheStore.Set(cacheItem2);

            Assert.AreEqual(cacheStore.CacheItemCount, 2, "cache item count mismatch");

            var keyList = cacheStore.GetKeys().ToList();

            Assert.AreEqual(keyList.Count, 2, "unexpected number of keys");

            Assert.IsTrue(keyList.Contains(cacheItem1.Key), "key not found");
            Assert.IsTrue(keyList.Contains(cacheItem2.Key), "key not found");
        }
    }
}
