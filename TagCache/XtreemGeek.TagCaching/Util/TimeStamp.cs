using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace XtreemGeek.TagCaching.Util
{
    /// <summary>
    /// timestamp utility
    /// </summary>
    internal static class TimeStamp
    {
        /// <summary>
        /// used to provide fine grained time stamp 
        /// Ticks provided by Stopwatch class is finer than Ticks available in DateTime.Now
        /// </summary>
        private static Stopwatch _StopWatch { get; set; }

        static TimeStamp()
        {
            if (Stopwatch.IsHighResolution)
            {
                _StopWatch = new Stopwatch();
                _StopWatch.Start();
            }
        }

        public static long GetTimeStamp()
        {
            if (_StopWatch != null)
            {
                return _StopWatch.ElapsedTicks;
            }
            else
            {
                return DateTime.Now.Ticks;
            }
        }
    }
}
