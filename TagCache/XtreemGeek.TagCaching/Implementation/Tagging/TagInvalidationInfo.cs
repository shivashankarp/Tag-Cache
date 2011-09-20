using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XtreemGeek.TagCaching.Util;

namespace XtreemGeek.TagCaching.Implementation.Tagging
{
    /// <summary>
    /// indicates a snapshot of invalidation request
    /// </summary>
    internal class TagInvalidationInfo
    {
        /// <summary>
        /// the timestamp when the invalidation request was issued
        /// </summary>
        public long ValidAfterTimeStamp { get; private set; }

        /// <summary>
        /// the AND list of tags (the cache items that match this tag list
        /// have to be removed)
        /// </summary>
        public List<TagInfo> TagInfoList { get; private set; }

        public static TagInvalidationInfo Create(List<TagInfo> tagInfoList)
        {
            return new TagInvalidationInfo
            {
                TagInfoList = tagInfoList,
                ValidAfterTimeStamp = TimeStamp.GetTimeStamp()
            };
        }
    }
}
