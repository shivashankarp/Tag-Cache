using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace XtreemGeek.TagCaching.Util
{
    /// <summary>
    /// timer thread utility
    /// </summary>
    internal class IntervalTimer
    {
        private TimeSpan _IntervalSpan { get; set; }

        private Timer _IntervalTimer { get; set; }

        public event Action OnInterval;

        public IntervalTimer(TimeSpan intervalSpan)
        {
            _IntervalSpan = intervalSpan;
            _CreateTimer();
        }

        private void _CreateTimer()
        {
            if (_IntervalTimer != null)
            {
                _IntervalTimer.Dispose();
                _IntervalTimer = null;
            }

            _IntervalTimer = new Timer(_Run, null, (long)_IntervalSpan.TotalMilliseconds, Timeout.Infinite);
        }

        private void _Run(object state)
        {
            try
            {
                OnInterval();
            }
            catch
            {
            }

            _CreateTimer();
        }
    }
}
