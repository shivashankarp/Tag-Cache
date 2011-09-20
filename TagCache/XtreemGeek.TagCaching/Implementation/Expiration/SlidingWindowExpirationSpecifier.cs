using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XtreemGeek.TagCaching.Util;

namespace XtreemGeek.TagCaching.Implementation.Expiration
{
    /// <summary>
    /// An expiration specifier that works based on sliding window timespan
    /// </summary>
    public class SlidingWindowExpirationSpecifier : IExpirationSpecifier
    {
        private TimeSpan _WindowSpan { get; set; }

        private DateTime _UtcLastAccessed { get; set; }

        public SlidingWindowExpirationSpecifier(TimeSpan windowSpan)
        {
            _WindowSpan = windowSpan;
            Guard.Assert(_WindowSpan.TotalSeconds > 0, "invalid window span");
        }

        public bool Expired
        {
            get {  return DateTime.UtcNow - _UtcLastAccessed > _WindowSpan; }
        }

        public void OnCacheItemAccessed()
        {
            _UtcLastAccessed = DateTime.UtcNow;
        }
    }
}
