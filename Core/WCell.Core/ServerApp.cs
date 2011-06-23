using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Threading;
using Cell.Core;
using NLog;
using WCell.Core.Database;
using WCell.Core.Initialization;
using WCell.Core.Localization;
using WCell.Core.Timers;
using WCell.Util;
using WCell.Util.NLog;
using WCell.Util.Variables;
using WCell.Core.Addons;

namespace WCell.Core
{
	public abstract class ServerApp<T> : ServerBase
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

		private static readonly SelfRunningTaskQueue ioQueue = new SelfRunningTaskQueue(100, @"Server I\O - Queue", false);

		protected TimerEntry m_shutdownTimer;
		protected static InitMgr s_initMgr;

		protected ServerApp()
		{
			s_log.Debug(Resources.ServerStarting);

			AppUtil.AddApplicationExitHandler(_OnShutdown);
//#if !DEBUG
			AppDomain.CurrentDomain.UnhandledException +=
				(sender, args) => LogUtil.FatalException(args.ExceptionObject as Exception, Resources.FatalUnhandledException);
//#endif

			LogUtil.SystemInfoLogger = LogSystemInfo;
		}

		/// <summary>
		/// The singleton instance of the InitMgr that runs the default Startup routine.
		/// </summary>
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
					return string.Format("{0}.{1}.{2}.{3})", ver.Major, ver.Minor, ver.Build, ver.Revision);
				}
				return string.Format("[Cannot get AssemblyVersion]");
			}
		}

		/// <summary>
		/// Used for general I/O tasks.
		/// These tasks are usually blocking, so do not use this for precise timers
		/// </summary>
		public static SelfRunningTaskQueue IOQueue
		{
			get { return ioQueue; }
		}

		public override bool IsRunning
		{
			get { return _running; }
			set
			{
				if (_running != value)
				{
					_running = value;
					ioQueue.IsRunning = value;
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

		public void SetTitle(string title, params object[] args)
		{
			if (ConsoleActive)
			{
				Console.Title = string.Format(title, args);
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

		#region Start
		/// <summary>
		/// Is executed when the server finished starting up
		/// </summary>
		public static event Action Started;

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

				s_log.Info("Server started - Max Working Set Size: {0}", Process.GetCurrentProcess().MaxWorkingSet);
				//GC.Collect(2, GCCollectionMode.Optimized);
				UpdateTitle();

				var evt = Started;
				if (evt != null)
				{
					evt();
				}
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
			m_shutdownTimer = new TimerEntry((int) delayMillis, 0, upd =>
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
				ioQueue.RegisterUpdatable(m_shutdownTimer);
				IsPreparingShutdown = true;
			}
		}

		public virtual void CancelShutdown()
		{
			if (IsPreparingShutdown)
			{
				ioQueue.UnregisterUpdatable(m_shutdownTimer);
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