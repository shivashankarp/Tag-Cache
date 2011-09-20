using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XtreemGeek.TagCaching.Util;
using XtreemGeek.TagCaching.Implementation.Tagging;

namespace XtreemGeek.TagCaching.Implementation.Tagging
{
    /// <summary>
    /// captures the tags associated with a cache object
    /// </summary>
    internal class TagExpirationSpecifier : IExpirationSpecifier
    {
        private bool _expired;

        private long _CreationTimeStamp { get; set; }
        private List<TagInfo> _TagInfoList { get; set; }
        private List<object> _TagInvalidationTokenList { get; set; }

        public TagExpirationSpecifier(List<TagInfo> tagInfoList)
        {
            _TagInfoList = tagInfoList;
            _TagInvalidationTokenList = _TagInfoList.ConvertAll((ti) => ti.TagInvalidationInfoList.ListHeadToken);
            _CreationTimeStamp = TimeStamp.GetTimeStamp();
        }

        public bool Expired
        {
            get 
            {
                return _expired ? _expired : (_expired = _HasExpired());
            }
        }

        private bool _HasExpired()
        {
            // check whether any tag has been invalidated
            foreach (TagInfo tagInfo in _TagInfoList)
            {
                if (_CreationTimeStamp < tagInfo.ValidAfterTimeStamp)
                {
                    return true;
                }
            }            

			// check whether any tag combinations have been invalidated
            for (int i = 0; i < _TagInfoList.Count; i++)
            {
                TagInfo tagInfo = _TagInfoList[i];
                object token = _TagInvalidationTokenList[i];
                object newToken = tagInfo.TagInvalidationInfoList.ListHeadToken;    // take note of the new token before enumerating

                foreach (TagInvalidationInfo invalidationInfo in tagInfo.TagInvalidationInfoList.GetEnumerable(token))
                {
                    // first: time stamp should be older
                    if (_CreationTimeStamp < invalidationInfo.ValidAfterTimeStamp)
                    {
                        // second: check if the invalidation tag combination matches with the available tags 
                        if (_IsMatchedByTagSet(invalidationInfo.TagInfoList))
                        {
                            // this means that this item is captured by the invalidation and needs to expire, RIP
                            return true;
                        }
                    }
                }

                // update the list head token to point to the new token
                _TagInvalidationTokenList[i] = newToken;
            }

            return false;
        }

        private bool _IsMatchedByTagSet(List<TagInfo> tagSet)
        {
            if (_TagInfoList.Count < tagSet.Count)
            {
                return false;
            }

            for (int i = 0; i < tagSet.Count; i++)
            {
                if (!_TagInfoList.Contains(tagSet[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public void OnCacheItemAccessed()
        {
        }
    }
}
