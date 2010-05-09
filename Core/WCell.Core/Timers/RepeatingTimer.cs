/*************************************************************************
 *
 *   file		: RepeatingTimer.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-01-31 12:35:36 +0100 (to, 31 jan 2008) $
 *   last author	: $LastChangedBy: tobz $
 *   revision		: $Rev: 87 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Threading;

namespace WCell.Core.Timers
{
    /// <summary>
    /// A repeating timer that will continue to fire after its first
    /// tick until stopped manually.
    /// </summary>
    public class RepeatingTimer : IDisposable
    {
        private bool m_running;
        private Timer m_internalTimer;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="callback">the timer callback</param>
        public RepeatingTimer(TimerCallback callback)
        {
            m_internalTimer = new Timer(callback);
        }

        /// <summary>
        /// Whether or not the timer is running.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return m_running;
            }
        }

        /// <summary>
        /// Starts the timer, given the initial firing delay
        /// </summary>
        /// <param name="delay">the time before the timer fires in milliseconds</param>
        /// <param name="interval">the time between ticks after the initial tick</param>
        public void Start(int delay, int interval)
        {
            m_internalTimer.Change(delay, interval);
            m_running = true;
        }

        /// <summary>
        /// Starts the timer, given the initial firing delay
        /// </summary>
        /// <param name="delay">the time before the timer fires in milliseconds</param>
        /// <param name="interval">the time between ticks after the initial tick</param>
        public void Start(uint delay, uint interval)
        {
            m_internalTimer.Change(delay, interval);
            m_running = true;
        }

        /// <summary>
        /// Starts the timer, given the initial firing delay
        /// </summary>
        /// <param name="delay">the time before the timer fires in milliseconds</param>
        /// <param name="interval">the time between ticks after the initial tick</param>
        public void Start(long delay, long interval)
        {
            m_internalTimer.Change(delay, interval);
            m_running = true;
        }

        /// <summary>
        /// Starts the timer, given the initial firing delay
        /// </summary>
        /// <param name="delay">the time before the timer fires in TimeSpan format</param>
        /// <param name="interval">the time between ticks after the initial tick</param>
        public void Start(TimeSpan delay, TimeSpan interval)
        {
            m_internalTimer.Change(delay, interval);
            m_running = true;
        }

        /// <summary>
        /// Stops the timer
        /// </summary>
        public void Stop()
        {
            m_running = false;
            m_internalTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

		public void Dispose()
		{
			Stop();
			m_internalTimer.Dispose();
		}
    }
}