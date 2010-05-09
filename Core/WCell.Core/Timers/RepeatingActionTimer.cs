/*************************************************************************
 *
 *   file		: RepeatingActionTimer.cs
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
    /// Provides the base for strongly-typed, action-specific timers
    /// </summary>
    public abstract class RepeatingActionTimer
    {
        private Timer m_internalTimer;

        /// <summary>
        /// Default constructor
        /// </summary>
        public RepeatingActionTimer()
        {
            m_internalTimer = new Timer(TimerFire);
        }

        /// <summary>
        /// Starts the timer, given the initial firing delay
        /// </summary>
        /// <param name="delay">the time before the timer fires in milliseconds</param>
        /// <param name="interval">the time between ticks after the initial tick</param>
        public void Start(int delay, int interval)
        {
            m_internalTimer.Change(delay, interval);
        }

        /// <summary>
        /// Starts the timer, given the initial firing delay
        /// </summary>
        /// <param name="delay">the time before the timer fires in milliseconds</param>
        /// <param name="interval">the time between ticks after the initial tick</param>
        public void Start(uint delay, uint interval)
        {
            m_internalTimer.Change(delay, interval);
        }

        /// <summary>
        /// Starts the timer, given the initial firing delay
        /// </summary>
        /// <param name="delay">the time before the timer fires in milliseconds</param>
        /// <param name="interval">the time between ticks after the initial tick</param>
        public void Start(long delay, long interval)
        {
            m_internalTimer.Change(delay, interval);
        }

        /// <summary>
        /// Starts the timer, given the initial firing delay
        /// </summary>
        /// <param name="delay">the time before the timer fires in TimeSpan format</param>
        /// <param name="interval">the time between ticks after the initial tick</param>
        public void Start(TimeSpan delay, TimeSpan interval)
        {
            m_internalTimer.Change(delay, interval);
        }

        /// <summary>
        /// Stops the timer
        /// </summary>
        public void Stop()
        {
            m_internalTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void TimerFire(object state)
        {
            long nextFire = OnTick(state);
 
            m_internalTimer.Change(nextFire, Timeout.Infinite);
        }

        /// <summary>
        /// Called when the timer ticks
        /// </summary>
        /// <param name="state">the state object of the timer</param>
        /// <returns>the time before the timer should fire next, in milliseconds</returns>
        public abstract long OnTick(object state);
    }
}