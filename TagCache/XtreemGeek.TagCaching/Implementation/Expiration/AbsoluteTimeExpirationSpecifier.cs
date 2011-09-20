using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XtreemGeek.TagCaching.Util;

namespace XtreemGeek.TagCaching.Implementation.Expiration
{
    /// <summary>
    /// An expiration specifier that works based on absolute expiry time
    /// </summary>
    public class AbsoluteTimeExpirationSpecifier : IExpirationSpecifier
    {
        /// <summary>
        /// time is maintained in UTC to avoid ambiguities
        /// </summary>
        private DateTime _UtcExpiryTime { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="utcExpiryTime">Time is obtained in UTC to avoid ambiguities</param>
        public AbsoluteTimeExpirationSpecifier(DateTime utcExpiryTime)
        {
            Guard.Assert(utcExpiryTime.Kind == DateTimeKind.Utc, "only utc times are accepted");
            _UtcExpiryTime = utcExpiryTime;
        }

        public bool Expired
        {
            get { return DateTime.UtcNow > _UtcExpiryTime; }
        }

        public void OnCacheItemAccessed()
        {
        }
    }
}
