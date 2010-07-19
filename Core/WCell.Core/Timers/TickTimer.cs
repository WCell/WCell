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
	public class TickTimer : IDisposable, IUpdatable
	{
		private float _totalTime;

		private readonly float _maxTicks;
		private float _ticks;

		public float Interval;
		public Action<float> Action;

		public TickTimer()
		{
		}

		/// <summary>
		/// Creates a new timer with the given start delay, interval, and callback.
		/// </summary>
		/// <param name="delay">the delay before firing initially</param>
		/// <param name="interval">the interval between firing</param>
		/// <param name="callback">the callback to fire</param>
		public TickTimer(int interval, int ticks, Action<float> callback)
		{
			_totalTime = interval / 1000.0f;
			Action = callback;
			Interval = interval;
			_maxTicks = ticks;
		}

		/// <summary>
		/// Creates a new timer with the given start delay, interval, and callback.
		/// </summary>
		/// <param name="delay">the delay before firing initially</param>
		/// <param name="interval">the interval between firing</param>
		/// <param name="callback">the callback to fire</param>
		public TickTimer(uint interval, int ticks, Action<float> callback)
		{
			_totalTime = interval / 1000.0f;
			Action = callback;
			Interval = interval;
			_maxTicks = ticks;
		}

		/// <summary>
		/// Creates a new timer with the given start delay, interval, and callback.
		/// </summary>
		/// <param name="delay">the delay before firing initially</param>
		/// <param name="interval">the interval between firing</param>
		/// <param name="callback">the callback to fire</param>
		public TickTimer(float interval, int ticks, Action<float> callback)
		{
			_totalTime = interval;
			Action = callback;
			Interval = interval;
			_maxTicks = ticks;
		}

		/// <summary>
		/// The amount of time elapsed since the last firing.
		/// </summary>
		public float TotalTime
		{
			get { return _totalTime; }
		}

		/// <summary>
		/// Starts the time with the given delay and interval.
		/// </summary>
		/// <param name="initialDelay">the delay before firing initially</param>
		/// <param name="interval">the interval between firing</param>
		public void Start()
		{
			_ticks = 0;
			_totalTime = 0.0f;
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
			
			// update our idle time
			_totalTime += updateDelta;

			if (_totalTime >= Interval)
			{
				Action(_totalTime);
				if (_totalTime != -1)
				{
					_totalTime -= Interval;
					_ticks++;
					if (_ticks == _maxTicks)
					{
						Stop();
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

		public bool Equals(TimerEntry obj)
		{
			// needs to be improved
			return obj.Interval == Interval && Equals(obj.Action, Action);
		}

		public override bool Equals(object obj)
		{
			if (obj.GetType() != typeof(TimerEntry)) return false;
			return Equals((TimerEntry)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var result = ((int)(Interval * 397)) ^ (Action != null ? Action.GetHashCode() : 0);
				return result;
			}
		}
	}
}