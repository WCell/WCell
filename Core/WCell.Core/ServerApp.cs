using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using Cell.Core;
using Cell.Core.Collections;
using NLog;
using WCell.Core.Database;
using WCell.Core.Initialization;
using WCell.Core.Localization;
using WCell.Util.Threading;
using WCell.Core.Timers;
using WCell.Util;
using WCell.Util.NLog;
using WCell.Util.Variables;
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

		protected static int s_revision;
		protected static string s_entryLocation;
		protected static readonly string[] EmptyStringArr = new string[0];

		protected List<IUpdatable> m_updatables;
		protected WaitHandle m_waitHandle;
		protected LockfreeQueue<IMessage> m_messageQueue;

		protected int m_currentThreadId;
		protected Stopwatch m_queueTimer;
		protected long m_updateFrequency, m_lastUpdate;
		protected TimerEntry m_shutdownTimer;
		protected static InitMgr s_initMgr;

		protected ServerApp()
		{
			GetRevision();

			s_log.Debug(Resources.ServerStarting);

			AppUtil.AddApplicationExitHandler(_OnShutdown);
			AppDomain.CurrentDomain.UnhandledException +=
				(sender, args) => LogUtil.FatalException(args.ExceptionObject as Exception, Resources.FatalUnhandledException);

			LogUtil.SystemInfoLogger = LogSystemInfo;

			m_updatables = new List<IUpdatable>();
			m_waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
			m_messageQueue = new LockfreeQueue<IMessage>();
			m_queueTimer = Stopwatch.StartNew();
			m_updateFrequency = WCellDef.SERVER_UPDATE_INTERVAL;
			m_lastUpdate = 0;
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
		/// Gets the SVN revision for the current server build.
		/// </summary>
		[NotVariable]
		public static int Revision
		{
			get { return s_revision; }
			set { s_revision = value; }
		}

		/// <summary>
		/// Gets the string representation of the SVN revision for the current server build.
		/// </summary>
		public static string RevisionString
		{
			get { return s_revision == -1 ? "[Unknown]" : s_revision.ToString(); }
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
				_running = value;
				// start message loop
				ThreadPool.RegisterWaitForSingleObject(m_waitHandle, QueueUpdateCallback, null, m_updateFrequency, true);
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

		protected static void GetRevision()
		{
			var dir = Path.GetDirectoryName(EntryLocation);
			var svnDir = Path.Combine(dir, ".svn");
			if (!Directory.Exists(svnDir))
			{
				s_revision = -1;
			}
			else
			{
				s_revision = SvnUtil.GetVersionNumber(dir);
			}
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
			if (Thread.CurrentThread.ManagedThreadId != m_currentThreadId)
			{
				throw new InvalidOperationException("Not in context");
			}
		}

		/// <summary>
		/// Indicates whether the current Thread is the processor of the MessageQueue
		/// </summary>
		public bool IsInContext
		{
			get { return Thread.CurrentThread.ManagedThreadId == m_currentThreadId; }
		}

		/// <summary>
		/// Queues a task for execution in the server task pool.
		/// </summary>
		/// <param name="msg"></param>
		public void AddMessage(IMessage msg)
		{
			m_messageQueue.Enqueue(msg);
		}

		protected void QueueUpdateCallback(object state, bool timedOut)
		{
			if (!_running)
			{
				return;
			}

			m_currentThreadId = Thread.CurrentThread.ManagedThreadId;

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

			m_currentThreadId = 0;
			if (_running)
			{
				// re-register the Update-callback
				ThreadPool.RegisterWaitForSingleObject(m_waitHandle, QueueUpdateCallback, null, callbackTimeout, true);
			}
		}

		#endregion

		#region Start

		/// <summary>
		/// Starts the server and performs and needed initialization.
		/// </summary>
		public virtual void Start()
		{
			if (InitMgr.PerformInitialization())
			{
				var address = Utility.ParseOrResolve(Host);

				_tcpEndpoint = new IPEndPoint(address, Port);

				Start(true, false);

				if (!(_running = _tcpEnabled))
				{
					s_log.Fatal(Resources.InitFailed);
					Stop();
					return;
				}
				GC.Collect(2, GCCollectionMode.Optimized);
				SetTitle(ToString());
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
			IsShuttingDown = true;
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
			return string.Format("WCell {0},{1} (v{2})", GetType().Name, Revision > 0 ? " Rev. " + Revision : "", AssemblyVersion);
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
