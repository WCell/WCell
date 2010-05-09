/*************************************************************************
 *
 *   file		: OneShotTimer.cs
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
    /// A non-repeating timer that stops after it fires its first tick
    /// </summary>
    public class OneShotTimer
    {
        private Timer m_internalTimer;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="callback">the timer callback</param>
        public OneShotTimer(TimerCallback callback)
        {
            m_internalTimer = new Timer(callback);
        }

        /// <summary>
        /// Starts the timer, given the initial firing delay
        /// </summary>
        /// <param name="delay">the time before the timer fires in milliseconds</param>
        public void Start(int delay)
        {
            m_internalTimer.Change(delay, -1);
        }

        /// <summary>
        /// Starts the timer, given the initial firing delay
        /// </summary>
        /// <param name="delay">the time before the timer fires in milliseconds</param>
        public void Start(uint delay)
        {
            m_internalTimer.Change(delay, -1);
        }

        /// <summary>
        /// Starts the timer, given the initial firing delay
        /// </summary>
        /// <param name="delay">the time before the timer fires in milliseconds</param>
        public void Start(long delay)
        {
            m_internalTimer.Change(delay, -1);
        }

        /// <summary>
        /// Starts the timer, given the initial firing delay
        /// </summary>
        /// <param name="delay">the time before the timer fires in TimeSpan format</param>
        public void Start(TimeSpan delay)
        {
            m_internalTimer.Change(delay, TimeSpan.FromMilliseconds(-1));
        }

        /// <summary>
        /// Stops the timer
        /// </summary>
        public void Stop()
        {
            m_internalTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}