using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XtreemGeek.TagCaching;

namespace XtreemGeek.TagCaching.Implementation
{
    internal class CacheItem
    {
        public string Key { get; set; }

        public object Value { get; private set; }

        public int Priority { get; set; }

        public IExpirationSpecifier[] ExpirationSpecifierList { get; private set; }

        public DateTime LastAccessedTimeUtc { get; set; }

        public CacheItem(string key, object value, int priority, IExpirationSpecifier[] expirationSpecifierList)
        {
            this.Key = key;
            this.Value = value;
            this.Priority = priority;
            this.ExpirationSpecifierList = expirationSpecifierList;
        }
    }
}
