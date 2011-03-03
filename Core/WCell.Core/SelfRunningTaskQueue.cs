using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WCell.Core.Timers;
using WCell.Util.Collections;
using WCell.Util.NLog;
using WCell.Util.Threading;

namespace WCell.Core
{
	public class SelfRunningTaskQueue : IContextHandler
	{
		private bool _running;
		protected List<SimpleTimerEntry> m_timers;
		protected List<IUpdatable> m_updatables;
		protected LockfreeQueue<IMessage> m_messageQueue;

		protected Task _updateTask;
		protected int _currentUpdateThreadId;
		protected Stopwatch m_queueTimer;
		protected int m_updateInterval, m_lastUpdate;

		public SelfRunningTaskQueue(int updateInterval, string name, bool running = true)
		{
			Name = name;
			m_timers = new List<SimpleTimerEntry>();
			m_updatables = new List<IUpdatable>();
			m_messageQueue = new LockfreeQueue<IMessage>();
			m_queueTimer = Stopwatch.StartNew();
			m_updateInterval = updateInterval;
			m_lastUpdate = 0;

			IsRunning = running;
		}

		public string Name
		{
			get;
			set;
		}

		public bool IsRunning
		{
			get { return _running; }
			set
			{
				if (_running != value)
				{
					_running = value;
					if (value)
					{
						// start message loop
						_updateTask = Task.Factory.StartNewDelayed(m_updateInterval, QueueUpdateCallback, this);
					}
				}
			}
		}

		/// <summary>
		/// Update interval in milliseconds
		/// </summary>
		public int UpdateInterval
		{
			get { return m_updateInterval; }
			set { m_updateInterval = value; }
		}

		public long LastUpdateTime
		{
			get { return m_lastUpdate; }
		}

		#region Updatables
		/// <summary>
		/// Registers an updatable object in the server timer pool.
		/// </summary>
		/// <param name="updatable">the object to register</param>
		public void RegisterUpdatable(IUpdatable updatable)
		{
			AddMessage(() => m_updatables.Add(updatable));
		}

		/// <summary>
		/// Unregisters an updatable object from the server timer pool.
		/// </summary>
		/// <param name="updatable">the object to unregister</param>
		public void UnregisterUpdatable(IUpdatable updatable)
		{
			AddMessage(() => m_updatables.Remove(updatable));
		}


		/// <summary>
		/// Registers the given Updatable during the next Map Tick
		/// </summary>
		public void RegisterUpdatableLater(IUpdatable updatable)
		{
			m_messageQueue.Enqueue(new Message(() => RegisterUpdatable(updatable)));
		}

		/// <summary>
		/// Unregisters the given Updatable during the next Map Update
		/// </summary>
		public void UnregisterUpdatableLater(IUpdatable updatable)
		{
			m_messageQueue.Enqueue(new Message(() => UnregisterUpdatable(updatable)));
		}
		#endregion

		#region Timers
		public SimpleTimerEntry CallPeriodically(int delayMillis, Action callback)
		{
			var timer = new SimpleTimerEntry(delayMillis, callback, m_lastUpdate, false);
			m_timers.Add(timer);
			return timer;
		}

		public SimpleTimerEntry CallDelayed(int delayMillis, Action callback)
		{
			var timer = new SimpleTimerEntry(delayMillis, callback, m_lastUpdate, true);
			m_timers.Add(timer);
			return timer;
		}

		/// <summary>
		/// Stops running the given timer
		/// </summary>
		public void CancelTimer(SimpleTimerEntry entry)
		{
			m_timers.Remove(entry);
		}

		internal int GetDelayUntilNextExecution(SimpleTimerEntry timer)
		{
			return timer.Delay - (int)(LastUpdateTime - timer.LastCallTime);
		}
		#endregion

		#region Messages
		/// <summary>
		/// Queues a task for execution in the server task pool.
		/// </summary>
		public void AddMessage(Action action)
		{
			m_messageQueue.Enqueue(new Message(action));
		}

		public bool ExecuteInContext(Action action)
		{
			if (!IsInContext)
			{
				AddMessage(new Message(action));
				return false;
			}
			else
			{
				action();
				return true;
			}
		}

		public void EnsureContext()
		{
			if (Thread.CurrentThread.ManagedThreadId != _currentUpdateThreadId)
			{
				throw new InvalidOperationException("Not in context");
			}
		}

		/// <summary>
		/// Indicates whether the current Thread is the processor of the MessageQueue
		/// </summary>
		public bool IsInContext
		{
			get { return Thread.CurrentThread.ManagedThreadId == _currentUpdateThreadId; }
		}

		/// <summary>
		/// Queues a task for execution in the server task pool.
		/// </summary>
		/// <param name="msg"></param>
		public void AddMessage(IMessage msg)
		{
			m_messageQueue.Enqueue(msg);
		}

		protected void QueueUpdateCallback(object state)
		{
			try
			{
				if (!_running)
				{
					return;
				}

				if (Interlocked.CompareExchange(ref _currentUpdateThreadId, Thread.CurrentThread.ManagedThreadId, 0) == 0)
				{
					// get the time at the start of our task processing
					var timerStart = m_queueTimer.ElapsedMilliseconds;
					var updateDt = (int)(timerStart - m_lastUpdate);
					m_lastUpdate = (int)timerStart;

					// process updateables
					foreach (var updatable in m_updatables)
					{
						try
						{
							updatable.Update(updateDt);
						}
						catch (Exception e)
						{
							LogUtil.ErrorException(e, "Failed to update: " + updatable);
						}
					}

					// process timers
					var count = m_timers.Count;
					for (var i = count - 1; i >= 0; i--)
					{
						var timer = m_timers[i];
						if (GetDelayUntilNextExecution(timer) <= 0)
						{
							try
							{
								timer.Execute(this);
							}
							catch (Exception e)
							{
								LogUtil.ErrorException(e, "Failed to execute timer: " + timer);
							}
						}
					}

					// process messages
					IMessage msg;
					while (m_messageQueue.TryDequeue(out msg))
					{
						try
						{
							msg.Execute();
						}
						catch (Exception e)
						{
							LogUtil.ErrorException(e, "Failed to execute message: " + msg);
						}
						if (!_running)
						{
							return;
						}
					}

					// get the end time
					long timerStop = m_queueTimer.ElapsedMilliseconds;

					bool updateLagged = timerStop - timerStart > m_updateInterval;
					long callbackTimeout = updateLagged ? 0 : ((timerStart + m_updateInterval) - timerStop);

					Interlocked.Exchange(ref _currentUpdateThreadId, 0);
					if (_running)
					{
						// re-register the Update-callback
						_updateTask = Task.Factory.StartNewDelayed((int)callbackTimeout, QueueUpdateCallback, this);
					}
				}
			}
			catch (Exception ex)
			{
				LogUtil.ErrorException(ex, "Failed to run TaskQueue callback for \"{0}\"", Name);
			}
		}
		#endregion

		#region Waiting
		/// <summary>
		/// Ensures execution outside the Map-context.
		/// </summary>
		/// <exception cref="InvalidOperationException">thrown if the calling thread is the map thread</exception>
		public void EnsureNoContext()
		{
			if (Thread.CurrentThread.ManagedThreadId == _currentUpdateThreadId)
			{
				throw new InvalidOperationException(string.Format("Application Queue context prohibited."));
			}
		}

		/// <summary>
		/// Adds the given message to the map's message queue and does not return 
		/// until the message is processed.
		/// </summary>
		/// <remarks>Make sure that the map is running before calling this method.</remarks>
		/// <remarks>Must not be called from the map context.</remarks>
		public void AddMessageAndWait(bool allowInstantExecution, Action action)
		{
			AddMessageAndWait(allowInstantExecution, new Message(action));
		}

		/// <summary>
		/// Adds the given message to the map's message queue and does not return 
		/// until the message is processed.
		/// </summary>
		/// <remarks>Make sure that the map is running before calling this method.</remarks>
		/// <remarks>Must not be called from the map context.</remarks>
		public void AddMessageAndWait(bool allowInstantExecution, IMessage msg)
		{
			if (allowInstantExecution && IsInContext)
			{
				msg.Execute();
			}
			else
			{
				EnsureNoContext();

				// to ensure that we are not exiting in the current message-loop, add an updatable
				// which again registers the message
				var updatable = new SimpleUpdatable();
				updatable.Callback = () => AddMessage(new Message(() =>
				{
					msg.Execute();
					lock (msg)
					{
						Monitor.PulseAll(msg);
					}
					UnregisterUpdatable(updatable);
				}));

				lock (msg)
				{
					RegisterUpdatableLater(updatable);
					// int delay = this.GetWaitDelay();
					Monitor.Wait(msg);
					// Assert.IsTrue(added, string.Format(debugMsg, args));
				}
			}
		}

		/// <summary>
		/// Waits for one map tick before returning.
		/// </summary>
		/// <remarks>Must not be called from the map context.</remarks>
		public void WaitOneTick()
		{
			AddMessageAndWait(false, new Message(() =>
			{
				// do nothing
			}));
		}

		/// <summary>
		/// Waits for the given amount of ticks.
		/// One tick might take 0 until Map.UpdateSpeed milliseconds.
		/// </summary>
		/// <remarks>Make sure that the map is running before calling this method.</remarks>
		/// <remarks>Must not be called from the map context.</remarks>
		public void WaitTicks(int ticks)
		{
			EnsureNoContext();

			for (int i = 0; i < ticks; i++)
			{
				WaitOneTick();
			}
		}
		#endregion
	}
}
