using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace WCell.Core.Timers
{
	/// <summary>
	/// Lightweight timer object that supports one-shot or repeated firing.
	/// </summary>
	/// <remarks>This timer is not standalone, and must be driven via the <see cref="IUpdatable" /> interface.</remarks>
	public class TimerEntry : IDisposable, IUpdatable
	{
		private float _totalTime;

		public float RemainingInitialDelay, Interval;
		public Action<float> Action;

		public TimerEntry()
		{
		}

		/// <summary>
		/// Creates a new timer with the given start delay, interval, and callback.
		/// </summary>
		/// <param name="delay">the delay before firing initially</param>
		/// <param name="interval">the interval between firing</param>
		/// <param name="callback">the callback to fire</param>
		public TimerEntry(float delay, float interval, Action<float> callback)
		{
			_totalTime = -1.0f;
			Action = callback;
			RemainingInitialDelay = delay;
			Interval = interval;
		}

		/// <summary>
		/// Creates a new timer with the given start delay, interval, and callback.
		/// </summary>
		/// <param name="delay">the delay before firing initially</param>
		/// <param name="interval">the interval between firing</param>
		/// <param name="callback">the callback to fire</param>
		public TimerEntry(int delay, int interval, Action<float> callback)
			: this(delay / 1000.0f, interval / 1000.0f, callback)
		{
		}

		/// <summary>
		/// Creates a new timer with the given start delay, interval, and callback.
		/// </summary>
		/// <param name="delay">the delay before firing initially</param>
		/// <param name="interval">the interval between firing</param>
		/// <param name="callback">the callback to fire</param>
		public TimerEntry(uint delay, uint interval, Action<float> callback)
			: this(delay / 1000.0f, interval / 1000.0f, callback)
		{
		}

		public TimerEntry(Action<float> callback) : this(0,0,callback)
		{
		}

		/// <summary>
		/// The amount of time elapsed since the last firing.
		/// </summary>
		public float TotalTime
		{
			get { return _totalTime; }
		}

		/// <summary>
		/// Starts the timer.
		/// </summary>
		public void Start()
		{
			_totalTime = 0.0f;
		}

		/// <summary>
		/// Starts the timer with the given delay.
		/// </summary>
		/// <param name="initialDelay">the delay before firing initially</param>
		public void Start(float initialDelay)
		{
			RemainingInitialDelay = initialDelay;
			_totalTime = 0.0f;
		}

		/// <summary>
		/// Starts the time with the given delay and interval.
		/// </summary>
		/// <param name="initialDelay">the delay before firing initially</param>
		/// <param name="interval">the interval between firing</param>
		public void Start(float initialDelay, float interval)
		{
			RemainingInitialDelay = initialDelay;
			Interval = interval;
			_totalTime = 0.0f;
		}

		/// <summary>
		/// Starts the timer with the given delay.
		/// </summary>
		/// <param name="initialDelay">the delay before firing initially</param>
		public void Start(int initialDelay)
		{
			Start(initialDelay / 1000.0f);
		}

		/// <summary>
		/// Starts the time with the given delay and interval.
		/// </summary>
		/// <param name="initialDelay">the delay before firing initially</param>
		/// <param name="interval">the interval between firing</param>
		public void Start(int initialDelay, int interval)
		{
			Start(initialDelay / 1000.0f, interval / 1000.0f);
		}

		/// <summary>
		/// Starts the time with the given delay and interval.
		/// </summary>
		/// <param name="initialDelay">the delay before firing initially</param>
		/// <param name="interval">the interval between firing</param>
		/// <param name="callback">the callback to invoke after a lapse of the timer interval</param>
		public void Start(uint initialDelay, uint interval, Action<float> callback)
		{
			Action = callback;

			Start(initialDelay / 1000.0f, interval / 1000.0f);
		}
		
		/// <summary>
		/// Whether or not the timer is running.
		/// </summary>
		public bool IsRunning
		{
			get { return _totalTime >= 0; }
		}

		/// <summary>
		/// Stops the timer.
		/// </summary>
		public void Stop()
		{
			_totalTime = -1f;
		}

		/// <summary>
		/// Updates the timer, firing the callback if enough time has elapsed.
		/// </summary>
		/// <param name="updateDelta">the time change since the last update</param>
		public void Update(float updateDelta)
		{
			// means this timer is not running.
			if (_totalTime == -1f)
				return;

			if (RemainingInitialDelay > 0.0f)
			{
				RemainingInitialDelay -= updateDelta;

				if (RemainingInitialDelay <= 0.0f)
				{
                    if (Interval == 0.0f)
                    {
                        // we need to stop the timer if it's only
						// supposed to fire once.
                        Stop();
					}

                    Action(_totalTime);
					if (_totalTime != -1)
					{
						_totalTime = 0;
					}
                }
			}
			else
			{
				// update our idle time
				_totalTime += updateDelta;

				if (_totalTime >= Interval)
				{
					Action(_totalTime);
					if (_totalTime != -1)
					{
						_totalTime -= Interval;
					}
				}
			}
		}

		/// <summary>
		/// Stops and cleans up the timer.
		/// </summary>
		public void Dispose()
		{
			Stop();
			Action = null;
		}

		public override bool Equals(object obj)
		{
			if (obj.GetType() != typeof (TimerEntry)) return false;
			return Equals((TimerEntry) obj);
		}

		public bool Equals(TimerEntry obj)
		{
			// needs to be improved
			return obj.Interval == Interval && Equals(obj.Action, Action);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var result = ((int)(Interval*397)) ^ (Action != null ? Action.GetHashCode() : 0);
				return result;
			}
		}
	}
}
