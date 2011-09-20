using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using XtreemGeek.TagCaching.Util;

namespace XtreemGeek.TagCaching.Implementation.Tagging
{
    /// <summary>
    /// Maintains a weak-ref map of tag to tag-info.
    /// This is a weak-ref map so that we know when to remove tags from this map.
    /// 
    /// The weak-ref would become null when there are no cache objects that are using
    /// the tag anymore.  So, when we find the weak-ref to be null, we remove the tag 
    /// from the map.  This way we prevent this dictionary from constantly growing due to 
    /// unused tags.
    /// </summary>
    internal class TagInfoMap
    {
        /// <summary>
        /// We use this object to synchronize adding-to-map and remove-from-map
        /// as the two happening simultaneously may lead to incorrect TagInfo objects
        /// </summary>
        private object _tagMapUpdateSync = new object();

        private IntervalTimer _IntervalTimer { get; set; }
        private ConcurrentDictionary<string, WeakReference> _TagInfoByTag { get; set; }

        public TagInfoMap(CacheOptions cacheOptions)
        {
            _TagInfoByTag = new ConcurrentDictionary<string, WeakReference>();

            _IntervalTimer = new IntervalTimer(cacheOptions.TagMapScavengeInterval);
            _IntervalTimer.OnInterval += new Action(_ScavengeTagMap);
        }

        public List<TagInfo> GetTagInfoList(List<string> tagList)
        {
            List<TagInfo> tagInfoList = new List<TagInfo>(tagList.Count);

            foreach (string tag in tagList)
            {
                TagInfo tagInfo = _GetTagInfoByTag(tag);

                if (tagInfo == null)
                {
                    // lock to ensure that another thread doesn't add this tagInfo 
                    lock (_tagMapUpdateSync)
                    {
                        // get and check to ensure that tagInfo is still null
                        tagInfo = _GetTagInfoByTag(tag);
                        
                        if (tagInfo == null)
                        {
                            tagInfo = new TagInfo(tag);
                            _TagInfoByTag[tag] = new WeakReference(tagInfo);
                        }
                    }
                }

                Guard.Assert(tagInfo != null, "tagInfo cannot be null");
                tagInfoList.Add(tagInfo);
            }

            Guard.Assert(tagInfoList.Count == tagList.Count, "BUGBUG: tag list count doesn't match with tag info list!");

            return tagInfoList;
        }

        /// <summary>
        /// List&lt;List&lt;string&gt;&gt; is used similar to sum-of-product expression.
        /// The outer list signifies OR and the inner list signifies AND.
        /// </summary>
        /// <param name="tagExpression"></param>
        public void SetInvalidateByTagExpression(List<List<string>> tagExpression)
        {
            // first optimize the invalidation for single tag sub expression
            foreach (List<string> subExpression in tagExpression)
            {
                if (subExpression.Count == 1)
                {
                    TagInfo tagInfo = _GetTagInfoByTag(subExpression[0]);
                    if (tagInfo != null)
                    {
                        tagInfo.UpdateTimeStamp();
                    }
                }
            }

            foreach (List<string> subExpression in tagExpression)
            {
                if (subExpression.Count > 1)
                {
                    List<TagInfo> tagInfoList = subExpression.ConvertAll(tag => _GetTagInfoByTag(tag));

                    if (tagInfoList.Exists((ti) => ti == null))
                    {
                        // at least one of the tags doesn't exist in the list
                        // means that no item related to this combination exist in the cache
                        // so, we can safely ignore this sub expression
                        continue;
                    }

                    // we always append the tag invalidation to the last tag in the list, under the assumption that
                    // the last tag would map to smaller number of items compared to the former tags
                    // this assumption however holds good only if the users of this cache create tag lists in such order
                    TagInfo tagInfo = tagInfoList[tagInfoList.Count - 1];
                    tagInfo.AddTagInvalidationInfo(tagInfoList);
                }
            }
        }

        private TagInfo _GetTagInfoByTag(string tag)
        {
            WeakReference tagInfoWeakRef;
            _TagInfoByTag.TryGetValue(tag, out tagInfoWeakRef);

            TagInfo tagInfo = tagInfoWeakRef != null ? (TagInfo)tagInfoWeakRef.Target : null;
            return tagInfo;
        }

        /// <summary>
        /// for all those weak-refs that are nulls we can remove the tag entry from the map
        /// </summary>
        private void _ScavengeTagMap()
        {
            ICollection<string> tagList = _TagInfoByTag.Keys;
            List<string> subsetTagList = new List<string>(tagList.Count);

            foreach(string tag in tagList)
            {
                WeakReference weakReference = _TagInfoByTag[tag];
                if (weakReference.Target == null)
                {
                    subsetTagList.Add(tag);
                }
            }

            // lock the map to ensure that no update happens while removing the nulled weak references
            lock (_tagMapUpdateSync)
            {
                foreach (string tag in subsetTagList)
                {
                    WeakReference weakReference = _TagInfoByTag[tag];
                    if (weakReference.Target == null)
                    {
                        _TagInfoByTag.TryRemove(tag, out weakReference);
                    }
                }
            }
        }
    }
}
