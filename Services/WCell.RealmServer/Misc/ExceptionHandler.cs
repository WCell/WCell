using System;
using WCell.Core.Initialization;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Global;
using WCell.Util.Collections;
using WCell.Util.Graphics;
using WCell.Util.NLog;

namespace WCell.RealmServer.Misc
{
	public static class ExceptionHandler
	{
		/// <summary>
		/// Sends exceptions to online staff with at least the given Rank.
		/// </summary>
		public static int ExceptionNotificationRank = 1000;
		private static int recentExceptions;

		private static readonly TimeSpan OneHour = TimeSpan.FromHours(1);

		private static double excepRaisingSpeed;
		private static DateTime lastExceptionTime;

		public static TimeSpan TimeSinceLastException
		{
			get { return DateTime.Now - lastExceptionTime; }
		}

		public static readonly SynchronizedList<ExceptionInfo> Exceptions = new SynchronizedList<ExceptionInfo>();

		[Initialization(InitializationPass.Tenth)]
		public static void Init()
		{
			LogUtil.ExceptionRaised += OnException;
		}

		private static void OnException(string msg, Exception ex)
		{
			if (ex != null)
			{
				Exceptions.Add(new ExceptionInfo(msg, ex));
				var delay = TimeSinceLastException.TotalMinutes;
				if (delay > 60)
				{
					excepRaisingSpeed = 1;
					recentExceptions = 0;
				}
				else
				{
					++recentExceptions;
					excepRaisingSpeed = (3 * excepRaisingSpeed + (1 / delay)) / 4;
				}

				if (recentExceptions > 5 && excepRaisingSpeed > 50 && !RealmServer.IsShuttingDown)
				{
					//World.Broadcast("[Warning] Server has become unstable...");
					//RealmServer.Instance.ShutdownIn(5000);
					return;
				}
				lastExceptionTime = DateTime.Now;
			}

			NotifyException(msg, ex);
		}

		private static void NotifyException(string msg, Exception ex)
		{
			foreach (var chr in World.GetAllCharacters())
			{
				if (chr.Role >= ExceptionNotificationRank)
				{
					if (ex != null)
					{
						chr.SendSystemMessage(ChatUtility.Colorize("Exception raised: ", Color.Red));
					}

					chr.SendSystemMessage(ChatUtility.Colorize(msg, Color.Red));
					if (ex != null)
					{
						chr.SendSystemMessage(ChatUtility.Colorize(ex.Message, Color.Red));
					}
				}
			}
		}
	}

	public class ExceptionInfo
	{
		public readonly DateTime Time = DateTime.Now;
		public readonly Exception Exception;
		public readonly string Message;

		public ExceptionInfo(string msg, Exception exception)
		{
			Message = msg;
			Exception = exception;
		}

		public override string ToString()
		{
			return Exception.Message + " triggered at " + Time + " - " + Message;
		}
	}
}