/*************************************************************************
 *
 *   file		: Manager.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-02-16 05:30:51 +0100 (ma, 16 feb 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 757 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using NLog;
using Cell.Core;
using WCell.Core.Localization;
using WCell.Util.NLog;

namespace WCell.Core
{
	/// <summary>
	/// Manager states enum used to determinate the state of the manager in a specific momment
	/// </summary>
	public enum ManagerStates : byte
	{
		/// <summary>
		/// The manager has started and is running
		/// </summary>
		Started = 0,
		/// <summary>
		/// The manager is currently starting
		/// </summary>
		Starting = 1,
		/// <summary>
		/// The manager is stopped and not running
		/// </summary>
		Stopped = 2,
		/// <summary>
		/// The manager is currently stopping
		/// </summary>
		Stopping = 3,
		/// <summary>
		/// The manager has been restarted
		/// </summary>
		Restarting = 4,
		/// <summary>
		/// The manager suffered an error
		/// </summary>
		Error = 5
	}
	#region Manager Exceptions
	/// <summary>
	/// Base manager <see cref="Exception"/> class
	/// </summary>
	[Serializable]
	public class ManagerException : Exception
	{
		private readonly uint m_totalErrors;

		public uint TotalErrors
		{
			get { return m_totalErrors; }
		}

		public ManagerException() { }
		public ManagerException(uint totalErrors)
		{
			m_totalErrors = totalErrors;
		}
		public ManagerException(uint totalErrors, string message)
			: base(message)
		{
			m_totalErrors = totalErrors;
		}
		public ManagerException(uint totalErrors, string message, Exception inner)
			: base(message, inner)
		{
			m_totalErrors = totalErrors;
		}
		protected ManagerException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
			info.AddValue("Total Errors: ", m_totalErrors);
		}
	}
	#endregion

	#region Public Manager Event Args
	/// <summary>
	/// Base manager event arguments used all manager events.
	/// </summary>
	public class ManagerEventArgs : EventArgs
	{
		private readonly uint m_eventCounter;

	    public ManagerException Exception { get; set; }

	    public bool Succeeded { get; set; }

	    public uint EventCounter
		{
			get { return m_eventCounter; }
		}

		public ManagerEventArgs(uint eventCounter)
		{
			m_eventCounter = eventCounter;
			Succeeded = true;
			Exception = null;
		}

		public ManagerEventArgs(uint eventCounter, ManagerException exception)
		{
			m_eventCounter = eventCounter;
			Succeeded = (exception == null);
			Exception = exception;
		}
	}

	/// <summary>
	/// Manager restart event arguments used when raising the Restart event.
	/// </summary>
	public sealed class ManagerRestartEventArgs : ManagerEventArgs
	{
		private readonly bool m_forced;

		/// <summary>
		/// Indicates if the manager restart was forced.
		/// </summary>
		public bool Forced
		{
			get { return m_forced; }
		}

		public ManagerRestartEventArgs(uint eventCounter)
			: base(eventCounter)
		{
			m_forced = false;
		}

		public ManagerRestartEventArgs(uint eventCounter, bool forced)
			: base(eventCounter)
		{
			m_forced = forced;
		}

		public ManagerRestartEventArgs(uint eventCounter, bool forced, ManagerException exception)
			: base(eventCounter, exception)
		{
			m_forced = forced;
		}
	}

	/// <summary>
	/// Manager error event arguments used when raising the Error event.
	/// </summary>
	public sealed class ManagerErrorEventArgs : ManagerEventArgs
	{
		private readonly ManagerStates m_failedState;

		/// <summary>
		/// The current manager state where the error was caused
		/// </summary>
		public ManagerStates FailedState
		{
			get { return m_failedState; }
		}

		public ManagerErrorEventArgs(uint eventCounter, ManagerStates failedState)
			: base(eventCounter)
		{
			m_failedState = failedState;
		}

		public ManagerErrorEventArgs(uint eventCounter, ManagerStates failedState, ManagerException exception)
			: base(eventCounter, exception)
		{
			m_failedState = failedState;
		}
	}
	#endregion

	/// <summary>
	/// Base class used for all Manager classes
	/// </summary>
	/// <remarks>
	/// This base class implements the singleton design pattern thru deriving from a <see cref="Singleton{T}"/>
	/// This singleton class enforces the singleton pattern at runtime. So when you inherit from Manager
	/// make sure your class have a non public default (empty) constructor but dont have a public one.
	/// </remarks>
	/// <example>
	/// public sealed class ChatManager : Manager{ChatManager}
	/// {
	///     private ChatManager()
	///     {
	///     }
	/// }
	/// </example>
	/// <typeparam name="T">The <see cref="Type"/> of the class that we want to create a manager for.</typeparam>
	public abstract class Manager<T> : Singleton<T> where T : class
	{
		protected static readonly Logger s_log = LogManager.GetLogger(typeof(T).FullName);
		protected static readonly SpinWaitLock m_lock = new SpinWaitLock();
		private ManagerStates m_state = ManagerStates.Stopped;
		private uint m_startCounter = 0;
		private uint m_stopCounter = 0;
		private uint m_restartCounter = 0;
		private uint m_errorCounter = 0;

		#region Public Events
		/// <summary>
		/// Event called when the Manager has started
		/// </summary>
		public event EventHandler<ManagerEventArgs> Started;
		/// <summary>
		/// Event called when the Manager has succesfully stopped
		/// </summary>
		public event EventHandler<ManagerEventArgs> Stopped;
		/// <summary>
		/// Event called when the Manager has succesfully restarted
		/// </summary>
		public event EventHandler<ManagerRestartEventArgs> Restarted;
		/// <summary>
		/// Event called when the Manager is about to start
		/// </summary>
		public event EventHandler<ManagerEventArgs> Starting;
		/// <summary>
		/// Event called when the Manager is about to stop
		/// </summary>
		public event EventHandler<ManagerEventArgs> Stopping;
		/// <summary>
		/// Event called when the Manager is about to restart
		/// </summary>
		public event EventHandler<ManagerRestartEventArgs> Restarting;
		/// <summary>
		/// Event called when the Manager throws an error
		/// </summary>
		public event EventHandler<ManagerErrorEventArgs> Error;
		#endregion

		#region Public Properties
		/// <summary>
		/// The manager states enum.
		/// </summary>
		public ManagerStates State
		{
			get { return m_state; }
		}

		/// <summary>
		/// The ammount of times this manager was started.
		/// </summary>
		public uint StartCounter
		{
			get { return m_startCounter; }
		}
		/// <summary>
		/// The ammount of times this manager was stopped.
		/// </summary>
		public uint StopCounter
		{
			get { return m_stopCounter; }
		}
		/// <summary>
		/// The ammount of times this manager was restarted.
		/// </summary>
		public uint RestartCounter
		{
			get { return m_restartCounter; }
		}
		/// <summary>
		/// The ammount of errors generated in this manager.
		/// </summary>
		public uint ErrorCounter
		{
			get { return m_errorCounter; }
		}
		#endregion

		/// <summary>
		/// Starts the manager
		/// </summary>
		/// <returns>True if the manager started succesfully. False otherwise</returns>
		public bool Start()
		{
			m_lock.Enter();

			if (m_state == ManagerStates.Stopped)
			{
				try
				{
					m_state = ManagerStates.Starting;
					OnStarting();

					if (InternalStart())
					{
						m_startCounter++;
						m_state = ManagerStates.Started;
						OnStarted(null);
						return true;
					}
					else
					{
						m_errorCounter++;
						OnError(new ManagerException(m_errorCounter, string.Format(Resources.ManagerInternalStartFailed, GetType().Name)));
						m_state = ManagerStates.Error;
						return false;
					}
				}
				catch (Exception e)
				{
					m_errorCounter++;
					OnError(new ManagerException(m_errorCounter, string.Format(Resources.ManagerStartFailed, GetType().Name), e));
					m_state = ManagerStates.Error;
					return false;
				}
				finally
				{
					m_lock.Exit();
				}
			}

		    return false;
		}

		/// <summary>
		/// Stops the manager
		/// </summary>
		/// <returns>True if the manager stopped succesfully. False otherwise</returns>
		public bool Stop()
		{
			m_lock.Enter();

			if (m_state == ManagerStates.Started)
			{
				try
				{
					m_state = ManagerStates.Stopping;
					OnStopping();

					if (InternalStop())
					{
						m_stopCounter++;
						m_state = ManagerStates.Stopped;
						OnStopped(null);
						return true;
					}
					else
					{
						m_errorCounter++;
						OnError(new ManagerException(m_errorCounter, Resources.ManagerInternalStopFailed));
						m_state = ManagerStates.Error;
						return false;
					}
				}
				catch (Exception e)
				{
					m_errorCounter++;
					OnError(new ManagerException(m_errorCounter, Resources.ManagerStopFailed, e));
					m_state = ManagerStates.Error;
					return false;
				}
				finally
				{
					m_lock.Exit();
				}
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Restarts the manager
		/// </summary>
		/// <param name="forced">If true, forces the restart of the manager even if its in an 
		/// <see cref="ManagerStates.Error"/> state.</param>
		/// <returns>True if the manager restarted succesfully. False otherwise</returns>
		public bool Restart(bool forced)
		{
			m_lock.Enter();

			try
			{
				if ((m_state != ManagerStates.Error) || (m_state == ManagerStates.Error && forced))
				{
					m_state = ManagerStates.Restarting;
					OnRestarting(forced);

					if (InternalRestart(forced))
					{
						m_restartCounter++;
						m_state = ManagerStates.Started;
						OnRestarted(forced, null);
						return true;
					}
					else
					{
						m_errorCounter++;
						OnError(new ManagerException(m_errorCounter, Resources.ManagerInternalRestartFailed));
						m_state = ManagerStates.Error;
						return false;
					}
				}
				else
					//Throw an exception ??
					return false;
			}
			catch (Exception e)
			{
				m_errorCounter++;
				OnError(new ManagerException(m_errorCounter, Resources.ManagerRestartFailed, e));
				m_state = ManagerStates.Error;
				return false;
			}
			finally
			{
				m_lock.Exit();
			}
		}

		/// <summary>
		/// Called on server start to allow derived managers to load its custom state properly
		/// </summary>
		/// <returns>True if the derived manager could load its state correctly. False otherwise</returns>
		protected abstract bool InternalStart();

		/// <summary>
		/// Called on server stop to allow derived managers to unload its custom state properly
		/// </summary>
		/// <returns>True if the derived manager could unload its state correctly. False otherwise</returns>
		protected abstract bool InternalStop();

		/// <summary>
		/// Called on server start to allow derived managers to restart its custom state properly
		/// </summary>
		/// <returns>True if the derived manager could restart its state correctly. False otherwise</returns>
		protected virtual bool InternalRestart(bool forced)
		{
			return InternalStart() && InternalStop();
		}

		/// <summary>
		/// Triggers the <see cref="Starting"/> event
		/// </summary>
		protected virtual void OnStarting()
		{
			s_log.Info(Resources.ManagerStarting, typeof(T).Name);

			EventHandler<ManagerEventArgs> handler = Starting;

			if (handler != null)
				handler(this, new ManagerEventArgs(m_startCounter, null));
		}

		/// <summary>
		/// Triggers the <see cref="Stopping"/> event
		/// </summary>
		protected virtual void OnStopping()
		{
			s_log.Info(Resources.ManagerStopping, typeof(T).Name);

			EventHandler<ManagerEventArgs> handler = Stopping;

			if (handler != null)
				handler(this, new ManagerEventArgs(m_stopCounter, null));
		}

		/// <summary>
		/// Triggers the <see cref="Restarting"/> event
		/// <param name="forced">If true, forces the restart of the manager even if its in an 
		/// <see cref="ManagerStates.Error"/> state.</param>
		/// </summary>
		protected virtual void OnRestarting(bool forced)
		{
			s_log.Info(Resources.ManagerRestarting, typeof(T).Name);

			EventHandler<ManagerRestartEventArgs> handler = Restarting;

			if (handler != null)
			{
				handler(this, new ManagerRestartEventArgs(m_restartCounter, forced, null));
			}
		}

		/// <summary>
		/// Triggers the <see cref="Started"/> event
		/// </summary>
		/// <param name="exception">The exception thrown when processing the start of the manager, if it failed.
		/// Null if the manager was started succesfully.
		/// </param>
		protected virtual void OnStarted(ManagerException exception)
		{
			s_log.Info(Resources.ManagerStarted, typeof(T).Name);

			EventHandler<ManagerEventArgs> handler = Started;

			if (handler != null)
			{
				handler(this, new ManagerEventArgs(m_startCounter, exception));
			}
		}

		/// <summary>
		/// Triggers the <see cref="Stopped"/> event
		/// </summary>
		/// <param name="exception">The exception thrown when processing the stop of the manager, if it failed.
		/// Null if the manager was stopped succesfully.
		/// </param>
		protected virtual void OnStopped(ManagerException exception)
		{
			s_log.Info(Resources.ManagerStopped, typeof(T).Name);

			EventHandler<ManagerEventArgs> handler = Stopped;

			if (handler != null)
			{
				handler(this, new ManagerEventArgs(m_stopCounter, exception));
			}
		}

		/// <summary>
		/// Triggers the <see cref="Restarted"/> event
		/// </summary>
		/// <param name="forced">True if the manager restart was forced. False otherwise</param>
		/// <param name="exception">The exception thrown when processing the restart of the manager, if it failed.
		/// Null if the manager was restarted succesfully.
		/// </param>
		protected virtual void OnRestarted(bool forced, ManagerException exception)
		{
			s_log.Info(Resources.ManagerRestarted, typeof(T).Name);

			EventHandler<ManagerRestartEventArgs> handler = Restarted;

			if (handler != null)
			{
				handler(this, new ManagerRestartEventArgs(m_restartCounter, forced, exception));
			}
		}

		/// <summary>
		/// Triggers the <see cref="Error"/> event.
		/// </summary>
		/// <param name="exception">The exception that caused the error</param>
		protected virtual void OnError(ManagerException exception)
		{
			LogUtil.ErrorException(exception, false, string.Format(Resources.ManagerThrownError, this, typeof(T).FullName));

			EventHandler<ManagerErrorEventArgs> handler = Error;

			if (handler != null)
			{
				handler(this, new ManagerErrorEventArgs(m_errorCounter, m_state, exception));
			}
		}

		public override string ToString()
		{
			return GetType().Name;
		}
	}
}