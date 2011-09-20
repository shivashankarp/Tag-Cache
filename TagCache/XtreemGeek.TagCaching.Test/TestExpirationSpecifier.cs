using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XtreemGeek.TagCaching.Test
{
    internal class TestExpirationSpecifier : IExpirationSpecifier
    {
        public bool Expired { get; set; }

        public void OnCacheItemAccessed()
        {
        }
    }
}
