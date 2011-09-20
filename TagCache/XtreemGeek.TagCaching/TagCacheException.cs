using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace XtreemGeek.TagCaching
{
    [Serializable]
    public class TagCacheException : Exception
    {
        public TagCacheException()
        {
        }

        public TagCacheException(string message)
            : base(message)
        {
        }

        public TagCacheException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public TagCacheException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
