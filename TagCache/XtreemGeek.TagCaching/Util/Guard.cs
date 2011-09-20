using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XtreemGeek.TagCaching.Util
{
    internal static class Guard
    {
        public static void Assert(bool condition, string format, params object[] args)
        {
            if (!condition)
            {
                throw new TagCacheException(String.Format(format, args));
            }
        }
    }
}
