//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using WCell.Core.Timers;
//using System.Threading;

//namespace WCell.Core.Timers
//{
//    //public class TickTimer : ITickTimer, IDisposable
//    {
//        protected bool m_alive;
//        protected int m_duration;
//        protected int m_amplitude;
//        protected int m_until;
//        protected int m_nextTick;
//        protected int m_maxTicks;
//        protected int m_ticks;
//        protected RepeatingTimer m_timer;

//        public TickTimer(ThreadStart tickDlg)
//        {
//            m_timer = new RepeatingTimer(tickDlg);
//            m_alive = true;
//        }

//        /// <summary>
//        /// Starts the timer.
//        /// Duration should be a multiple of amplitude
//        /// </summary>
//        protected void Start(int duration, int amplitude)
//        {
//            var now = Environment.TickCount;

//            m_duration = duration;
//            if (amplitude == 0) {
//                m_amplitude = m_duration;
//                m_maxTicks = 1;
//            }
//            else {
//                m_amplitude = amplitude;
//                m_maxTicks = (m_duration / m_amplitude);
//            }
//            m_until = now + m_duration;
//            m_nextTick = now + m_amplitude;
//            m_ticks = 0;

//            m_timer.Start(duration, Timeout.Infinite);
//        }

//        /// <summary>
//        /// The total amount of milliseconds remaining
//        /// </summary>
//        public int TimeLeft
//        {
//            get
//            {
//                return m_until - Environment.TickCount;
//            }
//        }

//        /// <summary>
//        /// The Environment.TickCount of the next Tick
//        /// </summary>
//        public int NextTick
//        {
//            get;
//        }

//        /// <summary>
//        /// The Environment.TickCount of the last Tick (Timer will close afterwards)
//        /// </summary>
//        public int Until
//        {
//            get;
//            set;
//        }

//        /// <summary>
//        /// The average amplitude between ticks (in ms)
//        /// </summary>
//        public int Amplitude
//        {
//            get;
//        }

//        /// <summary>
//        /// The total duration of this Timer (in ms)
//        /// </summary>
//        public int Duration
//        {
//            get;
//        }

//        /// <summary>
//        /// The amount of ticks that already passed
//        /// </summary>
//        public int Ticks
//        {
//            get;
//            set;
//        }

//        /// <summary>
//        /// The maximum amount of ticks
//        /// </summary>
//        public int MaxTicks
//        {
//            get;
//            set;
//        }

//        public virtual void Dispose()
//        {
//            if (!m_alive) {
//                return;
//            }
//            m_alive = false;

//            m_timer.Dispose();
//        }
//    }
//}
