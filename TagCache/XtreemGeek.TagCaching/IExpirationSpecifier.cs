using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XtreemGeek.TagCaching
{
    /// <summary>
    /// Custom expiration specifiers for TagCache can be
    /// implemented by extending this interface.
    /// </summary>
    public interface IExpirationSpecifier
    {
        /// <summary>
        /// indicates whether the cache item has expired
        /// </summary>
        bool Expired { get; }

        /// <summary>
        /// Called whenever the associated cache item is 
        /// accessed.  Can be used for audit.
        /// </summary>
        void OnCacheItemAccessed();
    }
}
