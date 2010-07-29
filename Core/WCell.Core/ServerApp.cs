using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Cell.Core;
using WCell.Util.Collections;
using NLog;
using WCell.Core.Database;
using WCell.Core.Initialization;
using WCell.Core.Localization;
using WCell.Util.Threading;
using WCell.Core.Timers;
using WCell.Util;
using WCell.Util.NLog;
using WCell.Util.Variables;
using WCell.Util.Threading.TaskParallel;
using IMessage = WCell.Util.Threading.IMessage;
using WCell.Core.Addons;

namespace WCell.Core
{
	public abstract class ServerApp<T> : ServerBase, IContextHandler
		where T : ServerBase, new()
	{
		[Variable]
		public static readonly DateTime StartTime;

		[Variable]
		public static TimeSpan RunTime
		{
			get { return DateTime.Now - StartTime; }
		}

		[NotVariable]
		public static bool ConsoleActive = true;

		static ServerApp()
		{
			StartTime = DateTime.Now;
		}

		protected static Logger s_log = LogManager.GetCurrentClassLogger();

		protected static string s_entryLocation;
		protected static readonly string[] EmptyStringArr = new string[0];

		protected List<IUpdatable> m_updatables;
		protected LockfreeQueue<IMessage> m_messageQueue;

		protected Task _updateTask;
		protected int _currentUpdateThreadId;
		protected Stopwatch m_queueTimer;
		protected long m_updateFrequency, m_lastUpdate;
		protected TimerEntry m_shutdownTimer;
		protected static InitMgr s_initMgr;

		protected ServerApp()
		{
			s_log.Debug(Resources.ServerStarting);

			AppUtil.AddApplicationExitHandler(_OnShutdown);
			AppDomain.CurrentDomain.UnhandledException +=
				(sender, args) => LogUtil.FatalException(args.ExceptionObject as Exception, Resources.FatalUnhandledException);

			LogUtil.SystemInfoLogger = LogSystemInfo;

			m_updatables = new List<IUpdatable>();
			m_messageQueue = new LockfreeQueue<IMessage>();
			m_queueTimer = Stopwatch.StartNew();
			m_updateFrequency = WCellDef.SERVER_UPDATE_INTERVAL;
			m_lastUpdate = 0;
		}

		protected void UpdateTitle()
		{
			SetTitle(ToString());
		}

		public void SetTitle(string title)
		{
			if (ConsoleActive)
			{
				Console.Title = title;
			}
		}

		public static InitMgr InitMgr
		{
			get
			{
				if (s_initMgr == null)
				{
					s_initMgr = new InitMgr(InitMgr.FeedbackRepeatFailHandler);
					s_initMgr.AddStepsOfAsm(Instance.GetType().Assembly);
				}
				return s_initMgr;
			}
		}

		/// <summary>
		/// Modify this to the Location of the file whose App-config you want to load.
		/// This is needed specifically for tests, since they don't have an EntryAssembly
		/// </summary>
		[NotVariable]
		public static string EntryLocation
		{
			get
			{
				if (s_entryLocation == null)
				{
					var asm = Assembly.GetEntryAssembly();
					if (asm != null)
					{
						s_entryLocation = asm.Location;
					}
				}

				return s_entryLocation;
			}
			set { s_entryLocation = value; }
		}

		/// <summary>
		/// Returns the single instance of the implemented server class.
		/// </summary>
		public static T Instance
		{
			get { return SingletonHolder<T>.Instance; }
		}

		/// <summary>
		/// Gets the assembly version information for the entry assembly of the process.
		/// </summary>
		public static string AssemblyVersion
		{
			get
			{
				var asm = Assembly.GetEntryAssembly();
				if (asm != null)
				{
					var ver = asm.GetName().Version;
					return string.Format("{0}.{1} ({2}#{3})", ver.Major, ver.Minor, ver.Build, ver.Revision);
				}
				return string.Format("[Cannot get AssemblyVersion]");
			}
		}

		public override bool IsRunning
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
						_updateTask = Task.Factory.StartNewDelayed((int)m_updateFrequency, QueueUpdateCallback, this);
					}
				}
			}
		}

		/// <summary>
		/// Whether the Server is in the process of shutting down (cannot be cancelled anymore)
		/// </summary>
		public static bool IsShuttingDown
		{
			get;
			private set;
		}

		/// <summary>
		/// Whether a timer has been started to shutdown the server.
		/// </summary>
		public static bool IsPreparingShutdown
		{
			get;
			private set;
		}

		public abstract string Host
		{
			get;
		}

		public abstract int Port
		{
			get;
		}

		private void LogSystemInfo(Action<string> logger)
		{
			var title = ToString();
#if DEBUG
			title += " - Debug";
#else
			title += " - Release";
#endif
			logger(title);
			logger(string.Format("OS: {0} - CLR: {1}", Environment.OSVersion, Environment.Version));
			logger(string.Format("Using: {0}", DatabaseUtil.Dialect != null ? DatabaseUtil.Dialect.GetType().Name : "<not initialized>"));
		}

		/// <summary>
		/// Gets the type from the App's own or any of the currently registered Addon Assemblies.
		/// </summary>
		/// <returns></returns>
		public static Type GetType(string name)
		{
			var type = Instance.GetType().Assembly.GetType(name);
			if (type == null)
			{
				foreach (var context in WCellAddonMgr.Contexts)
				{
					type = context.Assembly.GetType(name);
					if (type != null)
					{
						return type;
					}
				}
			}
			return type;
		}

		#region Timers

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
		/// Registers the given Updatable during the next Region Tick
		/// </summary>
		public void RegisterUpdatableLater(IUpdatable updatable)
		{
			m_messageQueue.Enqueue(new Message(() => RegisterUpdatable(updatable)));
		}

		/// <summary>
		/// Unregisters the given Updatable during the next Region Update
		/// </summary>
		public void UnregisterUpdatableLater(IUpdatable updatable)
		{
			m_messageQueue.Enqueue(new Message(() => UnregisterUpdatable(updatable)));
		}
		#endregion

		#region Task Pool

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
			if (!_running)
			{
				return;
			}

			if (Interlocked.CompareExchange(ref _currentUpdateThreadId, Thread.CurrentThread.ManagedThreadId, 0) == 0)
			{
				// get the time at the start of our task processing
				var timerStart = m_queueTimer.ElapsedMilliseconds;
				var updateDt = (timerStart - m_lastUpdate) / 1000.0f;

				// run timers!
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

				m_lastUpdate = m_queueTimer.ElapsedMilliseconds;

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

				bool updateLagged = timerStop - timerStart > m_updateFrequency;
				long callbackTimeout = updateLagged ? 0 : ((timerStart + m_updateFrequency) - timerStop);

				Interlocked.Exchange(ref _currentUpdateThreadId, 0);
				if (_running)
				{
					// re-register the Update-callback
					_updateTask = Task.Factory.StartNewDelayed((int)callbackTimeout, QueueUpdateCallback, this);
				}
			}
		}

		#region Waiting
		/// <summary>
		/// Ensures execution outside the Region-context.
		/// </summary>
		/// <exception cref="InvalidOperationException">thrown if the calling thread is the region thread</exception>
		public void EnsureNoContext()
		{
			if (Thread.CurrentThread.ManagedThreadId == _currentUpdateThreadId)
			{
				throw new InvalidOperationException(string.Format("Application Queue context prohibited."));
			}
		}

		/// <summary>
		/// Adds the given message to the region's message queue and does not return 
		/// until the message is processed.
		/// </summary>
		/// <remarks>Make sure that the region is running before calling this method.</remarks>
		/// <remarks>Must not be called from the region context.</remarks>
		public void AddMessageAndWait(bool allowInstantExecution, Action action)
		{
			AddMessageAndWait(allowInstantExecution, new Message(action));
		}

		/// <summary>
		/// Adds the given message to the region's message queue and does not return 
		/// until the message is processed.
		/// </summary>
		/// <remarks>Make sure that the region is running before calling this method.</remarks>
		/// <remarks>Must not be called from the region context.</remarks>
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
		/// Waits for one region tick before returning.
		/// </summary>
		/// <remarks>Must not be called from the region context.</remarks>
		public void WaitOneTick()
		{
			AddMessageAndWait(false, new Message(() =>
			{
				// do nothing
			}));
		}

		/// <summary>
		/// Waits for the given amount of ticks.
		/// One tick might take 0 until Region.UpdateSpeed milliseconds.
		/// </summary>
		/// <remarks>Make sure that the region is running before calling this method.</remarks>
		/// <remarks>Must not be called from the region context.</remarks>
		public void WaitTicks(int ticks)
		{
			EnsureNoContext();

			for (int i = 0; i < ticks; i++)
			{
				WaitOneTick();
			}
		}
		#endregion
		#endregion

		#region Start

		/// <summary>
		/// Starts the server and performs and needed initialization.
		/// </summary>
		public virtual void Start()
		{
			if (_running)
			{
				return;
			}

			if (InitMgr.PerformInitialization())
			{
				var address = Utility.ParseOrResolve(Host);

				_tcpEndpoint = new IPEndPoint(address, Port);

				Start(true, false);

				if (!(_running = TcpEnabledEnabled))
				{
					s_log.Fatal(Resources.InitFailed);
					Stop();
					return;
				}
				//GC.Collect(2, GCCollectionMode.Optimized);
				UpdateTitle();
			}
			else
			{
				s_log.Fatal(Resources.InitFailed);
				Stop();
			}
		}

		#endregion

		#region Shutdown

		/// <summary>
		/// Triggered when the App shuts down.
		/// </summary>
		public static event Action Shutdown;

		/// <summary>
		/// Forces the server to shutdown after the given delay.
		/// </summary>
		/// <param name="delayMillis">the time to wait before shutting down</param>
		public virtual void ShutdownIn(uint delayMillis)
		{
			m_shutdownTimer = new TimerEntry(delayMillis / 1000f, 0f, upd =>
			{
				AppUtil.UnhookAll();
				if (IsRunning)
				{
					_OnShutdown();
				}
			});

			m_shutdownTimer.Start();
			if (!IsPreparingShutdown)
			{
				RegisterUpdatable(m_shutdownTimer);
				IsPreparingShutdown = true;
			}
		}

		public virtual void CancelShutdown()
		{
			if (IsPreparingShutdown)
			{
				UnregisterUpdatable(m_shutdownTimer);
				m_shutdownTimer.Stop();
				IsPreparingShutdown = false;
			}
		}

		private void _OnShutdown()
		{
			if (IsShuttingDown)
			{
				return;
			}

			IsShuttingDown = true;

			var evt = Shutdown;
			if (evt != null)
			{
				evt();
			}

			OnShutdown();

			Stop();
			s_log.Info(Resources.ProcessExited);
			Thread.Sleep(1000);		// any last words?

			Process.GetCurrentProcess().CloseMainWindow();
		}

		protected virtual void OnShutdown()
		{
		}

		#endregion

		#region Singleton

		#endregion

		public override string ToString()
		{
			return string.Format("WCell {0} (v{1})", GetType().Name, AssemblyVersion);
		}
	}

	/// <summary>
	/// Private class for instances of a singleton object.
	/// </summary>
	/// <typeparam name="TSingle">the type of the singleton object</typeparam>
	static class SingletonHolder<TSingle> where TSingle : new()
	{
		internal static readonly TSingle Instance = new TSingle();
	}
}