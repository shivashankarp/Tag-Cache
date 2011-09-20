using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XtreemGeek.TagCaching.Util;

namespace XtreemGeek.TagCaching.Implementation.Tagging
{
    /// <summary>
    /// encapsulates 
    ///   1. individual tag expiry timestamp
    ///   2. invalidation time-stamp of tag expression that involves this tag
    /// </summary>
    internal class TagInfo
    {
        /// <summary>
        /// the tag for which this info object is being maintained
        /// </summary>
        public string Tag { get; private set; }

        /// <summary>
        /// timestamp after which this tag is valid
        /// </summary>
        public long ValidAfterTimeStamp { get; private set; }

        /// <summary>
        /// holds a subset of tag-expression invalidations that involve this tag
        /// </summary>
        public TailList<TagInvalidationInfo> TagInvalidationInfoList { get; private set; }

        public TagInfo(string tag)
        {
            Tag = tag;
            TagInvalidationInfoList = new TailList<TagInvalidationInfo>();
            UpdateTimeStamp();
        }

        public void AddTagInvalidationInfo(List<TagInfo> tagInfoList)
        {
            TagInvalidationInfoList.Add(TagInvalidationInfo.Create(tagInfoList));
        }

        public void UpdateTimeStamp()
        {
            ValidAfterTimeStamp = TimeStamp.GetTimeStamp();
        }

        public override bool Equals(object obj)
        {
            return Object.ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return Tag.GetHashCode();
        }
    }
}
