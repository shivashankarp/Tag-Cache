using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XtreemGeek.TagCaching.Implementation
{
    internal class CacheOptions
    {
        public TimeSpan ExpirationPollInterval { get; set; }

        public TimeSpan TagMapScavengeInterval { get; set; }

        public int MaxCacheItemCount { get; set; }

        public decimal ScavengePercentage { get; set; }

        public CacheOptions()
        {
            ExpirationPollInterval = TimeSpan.FromMinutes(2);
            TagMapScavengeInterval = TimeSpan.FromMinutes(5);
            MaxCacheItemCount = 25000;
            ScavengePercentage = 20M;
        }
    }
}
