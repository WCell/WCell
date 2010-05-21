using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System.Threading;
using System.Diagnostics;
using Cell.Core.Collections;
using WCell.Util.Threading;
using WCell.Util.Threading.TaskParallel;

namespace WCell.Core
{
	/// <summary>
	/// A task pool that processes messages asynchronously on the application thread pool.
	/// </summary>
	public class AsyncTaskPool
	{
		protected readonly Logger s_log = LogManager.GetCurrentClassLogger();
		protected LockfreeQueue<IMessage> m_taskQueue;
		protected Stopwatch m_taskTimer;
		protected long m_updateFrequency;

		/// <summary>
		/// Creates a new task pool with an update frequency of 100ms
		/// </summary>
		public AsyncTaskPool()
			: this(100)
		{
		}

		/// <summary>
		/// Creates a new task pool with the specified update frequency.
		/// </summary>
		/// <param name="updateFrequency">the update frequency of the task pool</param>
		public AsyncTaskPool(long updateFrequency)
		{
			m_taskQueue = new LockfreeQueue<IMessage>();
			m_taskTimer = Stopwatch.StartNew();
			m_updateFrequency = updateFrequency;

		    Task.Factory.StartNewDelayed((int)m_updateFrequency, TaskUpdateCallback, this);
		}

		/// <summary>
		/// Enqueues a new task in the queue that will be executed during the next
		/// tick.
		/// </summary>
		public void EnqueueTask(IMessage task)
		{
			if (task == null)
				throw new ArgumentNullException("task", "task cannot be null");

			m_taskQueue.Enqueue(task);
		}

		static readonly object obj = "";

		/// <summary>
		/// Waits until all currently enqueued messages have been processed.
		/// </summary>
		public void WaitOneTick()
		{
			var msg = new Message(() =>
			{
				lock (obj)
				{
					Monitor.PulseAll(obj);
				}
			});

			lock (obj)
			{
				m_taskQueue.Enqueue(msg);
				Monitor.Wait(obj);
			}
		}

		public void ChangeUpdateFrequency(long frequency)
		{
			if (frequency < 0)
				throw new ArgumentException("frequency cannot be less than 0", "frequency");

			m_updateFrequency = frequency;
		}

		protected void TaskUpdateCallback(object state)
		{
			// get the time at the start of our task processing
			long timerStart = m_taskTimer.ElapsedMilliseconds;

			ProcessTasks(timerStart);

			// get the end time
			long timerStop = m_taskTimer.ElapsedMilliseconds;

			bool updateLagged = timerStop - timerStart > m_updateFrequency;
			long callbackTimeout = updateLagged ? 0 : ((timerStart + m_updateFrequency) - timerStop);

			// re-register the update to be called
		    Task.Factory.StartNewDelayed((int)callbackTimeout, TaskUpdateCallback, this);
		}

		protected virtual void ProcessTasks(long startTime)
		{
			IMessage msg;

			// fire ze tasks
			while (m_taskQueue.TryDequeue(out msg))
			{
				msg.Execute();
			}
		}
	}
}