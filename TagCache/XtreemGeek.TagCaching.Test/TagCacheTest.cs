using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XtreemGeek.TagCaching.Test
{
    [TestClass]
    public class TagCacheTest
    {
        [TestMethod]
        public void TestSetGetInvalidate()
        {
            TagCache tagCache = new TagCache();

            Action fillCacheItems = delegate
            {
                tagCache.Set("honda", new object(), new List<string> { "Vehicle", "Car", "Economy" });
                tagCache.Set("lexus", new object(), new List<string> { "Vehicle", "Car", "Luxury" });
                tagCache.Set("harley", new object(), new List<string> { "Vehicle", "Bike", "Luxury" });
                tagCache.Set("yamaha", new object(), new List<string> { "Vehicle", "Bike", "Economy" });
            };

            fillCacheItems();

            // invalidate all bikes
            tagCache.Invalidate(new List<List<string>> { new List<string> { "Bike" } });
            Assert.IsNotNull(tagCache.Get("honda"));
            Assert.IsNotNull(tagCache.Get("lexus"));
            Assert.IsNull(tagCache.Get("harley"));
            Assert.IsNull(tagCache.Get("yamaha"));

            fillCacheItems();

            // invalidate all luxury vehicles
            tagCache.Invalidate(new List<List<string>> { new List<string> { "Luxury" } });
            Assert.IsNotNull(tagCache.Get("honda"));
            Assert.IsNull(tagCache.Get("lexus"));
            Assert.IsNull(tagCache.Get("harley"));
            Assert.IsNotNull(tagCache.Get("yamaha"));

            fillCacheItems();

            // invalidate all vehicles
            tagCache.Invalidate(new List<List<string>> { new List<string> { "Vehicle" } });
            Assert.IsNull(tagCache.Get("honda"));
            Assert.IsNull(tagCache.Get("lexus"));
            Assert.IsNull(tagCache.Get("harley"));
            Assert.IsNull(tagCache.Get("yamaha"));

            fillCacheItems();

            // invalidate all luxury cars
            tagCache.Invalidate(new List<List<string>> { new List<string> { "Car", "Luxury" } });
            Assert.IsNotNull(tagCache.Get("honda"));
            Assert.IsNull(tagCache.Get("lexus"));
            Assert.IsNotNull(tagCache.Get("harley"));
            Assert.IsNotNull(tagCache.Get("yamaha"));

            fillCacheItems();

            // invalidate all economy bikes
            tagCache.Invalidate(new List<List<string>> { new List<string> { "Bike", "Economy" } });
            Assert.IsNotNull(tagCache.Get("honda"));
            Assert.IsNotNull(tagCache.Get("lexus"));
            Assert.IsNotNull(tagCache.Get("harley"));
            Assert.IsNull(tagCache.Get("yamaha"));

            fillCacheItems();

            // invalidate all luxury bikes and economy cars
            tagCache.Invalidate(new List<List<string>> { new List<string> { "Bike", "Luxury" }, new List<string> { "Car", "Economy" } });
            Assert.IsNull(tagCache.Get("honda"));
            Assert.IsNotNull(tagCache.Get("lexus"));
            Assert.IsNull(tagCache.Get("harley"));
            Assert.IsNotNull(tagCache.Get("yamaha"));
        }

        [TestMethod]
        public void TestTagCacheAPIs()
        {
            TagCache tagCache = new TagCache();

            tagCache.Set("key", 1);
            tagCache.Set("key", 1, TagCache.LOWEST_PRIORITY);
            tagCache.Set("key", 1, DateTime.UtcNow.AddSeconds(10));
            tagCache.Set("key", 1, DateTime.UtcNow.AddSeconds(10), TagCache.LOWEST_PRIORITY);
            tagCache.Set("key", 1, TimeSpan.FromSeconds(10));
            tagCache.Set("key", 1, TimeSpan.FromSeconds(10), TagCache.LOWEST_PRIORITY);
            tagCache.Set("key", 1, new List<string> { "tag1", "tag2" });
            tagCache.Set("key", 1, new List<string> { "tag1", "tag2" }, TagCache.LOWEST_PRIORITY);
            tagCache.Set("key", 1, new List<string> { "tag1", "tag2" }, DateTime.UtcNow.AddSeconds(10));
            tagCache.Set("key", 1, new List<string> { "tag1", "tag2" }, DateTime.UtcNow.AddSeconds(10), TagCache.LOWEST_PRIORITY);
            tagCache.Set("key", 1, new List<string> { "tag1", "tag2" }, TimeSpan.FromSeconds(10));
            tagCache.Set("key", 1, new List<string> { "tag1", "tag2" }, TimeSpan.FromSeconds(10), TagCache.LOWEST_PRIORITY);

            Assert.AreEqual(tagCache.Get("key"), 1);
            tagCache.Remove("key");
            Assert.AreEqual(tagCache.Get("key"), null);

            tagCache.Set("key", 1, new List<string> { "tag1", "tag2" });
            Assert.AreEqual(tagCache.Get("key"), 1);
            tagCache.Invalidate(new List<List<string>> { new List<string> { "tag1" } });
            Assert.AreEqual(tagCache.Get("key"), null);
        }
    }
}
