using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using WCell.Util.Collections;
using WCell.Util.Threading;
using WCell.Util.Threading.TaskParallel;

namespace WCell.Core
{
	/// <summary>
	/// A task pool that processes messages asynchronously on the application thread pool.
	/// </summary>
	public class AsyncTaskPool
	{
		protected static readonly Logger Log = LogManager.GetCurrentClassLogger();
		protected LockfreeQueue<IMessage> TaskQueue;
		protected Stopwatch TaskTimer;
		protected long UpdateFrequency;

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
			TaskQueue = new LockfreeQueue<IMessage>();
			TaskTimer = Stopwatch.StartNew();
			UpdateFrequency = updateFrequency;

		    Task.Factory.StartNewDelayed((int)UpdateFrequency, TaskUpdateCallback, this);
		}

		/// <summary>
		/// Enqueues a new task in the queue that will be executed during the next
		/// tick.
		/// </summary>
		public void EnqueueTask(IMessage task)
		{
			if (task == null)
				throw new ArgumentNullException("task", "task cannot be null");

			TaskQueue.Enqueue(task);
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
				TaskQueue.Enqueue(msg);
				Monitor.Wait(obj);
			}
		}

		public void ChangeUpdateFrequency(long frequency)
		{
			if (frequency < 0)
				throw new ArgumentException("frequency cannot be less than 0", "frequency");

			UpdateFrequency = frequency;
		}

		protected void TaskUpdateCallback(object state)
		{
			// get the time at the start of our task processing
			long timerStart = TaskTimer.ElapsedMilliseconds;

			ProcessTasks(timerStart);

			// get the end time
			long timerStop = TaskTimer.ElapsedMilliseconds;

			bool updateLagged = timerStop - timerStart > UpdateFrequency;
			long callbackTimeout = updateLagged ? 0 : ((timerStart + UpdateFrequency) - timerStop);

			// re-register the update to be called
		    Task.Factory.StartNewDelayed((int)callbackTimeout, TaskUpdateCallback, this);
		}

		protected virtual void ProcessTasks(long startTime)
		{
			IMessage msg;

			// fire ze tasks
			while (TaskQueue.TryDequeue(out msg))
			{
				msg.Execute();
			}
		}
	}
}