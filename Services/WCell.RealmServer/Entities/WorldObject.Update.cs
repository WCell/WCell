using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Util;
using WCell.Util.Collections;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.Util.NLog;
using WCell.Util.Threading;
using WCell.Core.Timers;

namespace WCell.RealmServer.Entities
{
	public class ObjectUpdateTimer
	{
		public ObjectUpdateTimer() { }

		public ObjectUpdateTimer(int delayMillis, Action<WorldObject> callback)
		{
			Callback = callback;
			Delay = delayMillis;
			LastCallTime = DateTime.Now;
		}

		public DateTime LastCallTime
		{
			get;
			internal set;
		}

		public Action<WorldObject> Callback
		{
			get;
			set;
		}

		public int Delay
		{
			get;
			set;
		}

		public int GetDelayUntilNextExecution(WorldObject obj)
		{
			return Delay - (obj.LastUpdateTime - LastCallTime).ToMilliSecondsInt();
		}

		public void Execute(WorldObject owner)
		{
			Callback(owner);
			LastCallTime = owner.LastUpdateTime;
		}

		public override bool Equals(object obj)
		{
			return obj is ObjectUpdateTimer && Callback == ((ObjectUpdateTimer)obj).Callback;
		}

		public override int GetHashCode()
		{
			return Callback.GetHashCode();
		}
	}

	public class OneShotObjectUpdateTimer : ObjectUpdateTimer
	{
		public OneShotObjectUpdateTimer(int delayMillis, Action<WorldObject> callback)
		{
			Delay = delayMillis;
			Callback = obj =>
			{
				//if (!Cancelled)
				{
					callback(obj);
				}
				obj.RemoveUpdateAction(this);
			};
		}

		//public bool Cancelled
		//{
		//    get;
		//    set;
		//}
	}

	public partial class WorldObject
	{
		protected UpdatePriority m_UpdatePriority = DefaultObjectUpdatePriority;
		protected DateTime m_lastUpdateTime;

		protected List<ObjectUpdateTimer> m_updateActions;


		internal protected virtual void OnEnterMap()
		{
		}

		internal protected virtual void OnLeavingMap()
		{
		}

		/// <summary>
		/// The queue of messages. Messages are executed on every map tick.
		/// </summary>
		public LockfreeQueue<IMessage> MessageQueue
		{
			get { return m_messageQueue; }
		}

		public DateTime LastUpdateTime
		{
			get { return m_lastUpdateTime; }
			internal set { m_lastUpdateTime = value; }
		}

		#region Object Updating
		public override UpdatePriority UpdatePriority
		{
			get { return m_UpdatePriority; }
		}

		public void SetUpdatePriority(UpdatePriority priority)
		{
			m_UpdatePriority = priority;
		}

		public OneShotObjectUpdateTimer CallDelayed(int millis, Action<WorldObject> callback)
		{
			var action = new OneShotObjectUpdateTimer(millis, callback);
			AddUpdateAction(action);
			return action;
		}

		/// <summary>
		/// Adds a new Action to the list of Actions to be executed every millis.
		/// </summary>
		/// <param name="callback"></param>
		public ObjectUpdateTimer CallPeriodically(int millis, Action<WorldObject> callback)
		{
			var action = new ObjectUpdateTimer(millis, callback);
			AddUpdateAction(action);
			return action;
		}

		/// <summary>
		/// Adds a new Action to the list of Actions to be executed every action.Delay milliseconds
		/// </summary>
		public void AddUpdateAction(ObjectUpdateTimer timer)
		{
			if (m_updateActions == null)
			{
				m_updateActions = new List<ObjectUpdateTimer>(3);
			}
			timer.LastCallTime = m_lastUpdateTime;
			m_updateActions.Add(timer);
		}

		public bool HasUpdateAction(Func<ObjectUpdateTimer, bool> predicate)
		{
			EnsureContext();

			return m_updateActions != null && m_updateActions.Any(predicate);
		}

		public void RemoveUpdateAction(Action<WorldObject> callback)
		{
			if (m_updateActions != null)
			{
				ExecuteInContext(() =>
									{
										var action = m_updateActions.FirstOrDefault(act => act.Callback == callback);
										if (action != null)
										{
											RemoveUpdateAction(action);
										}
									});
			}
		}

		/// <summary>
		/// Removes the given Action
		/// </summary>
		/// <param name="timer"></param>
		public bool RemoveUpdateAction(ObjectUpdateTimer timer)
		{
			if (m_updateActions != null)
			{
				if (m_updateActions.Remove(timer))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Make sure to call this before updating anything else (required for reseting UpdateInfo)
		/// </summary>
		public virtual void Update(int dt)
		{
			IMessage msg;
			while (m_messageQueue.TryDequeue(out msg))
			{
				try
				{
					msg.Execute();
				}
				catch (Exception e)
				{
					LogUtil.ErrorException(e, "Exception raised when processing Message for: {0}", this);
					Delete();
				}
			}

			if (m_areaAuras != null)
			{
				var count = m_areaAuras.Count;
				for (var i = 0; i < count; i++)
				{
					var aura = m_areaAuras[i];
					aura.Update(dt);
					if (m_areaAuras.Count != count)
					{
						// areaauras changed -> break and continue on next tick
						break;
					}
				}
			}

			if (m_spellCast != null)
			{
				m_spellCast.Update(dt);
			}

			if (m_updateActions != null)
			{
				var count = m_updateActions.Count;
				for (var i = count - 1; i >= 0; i--)
				{
					var action = m_updateActions[i];
					if (action.Delay == 0)
					{
						// Keep an eye out for this...
						action.Execute(this);
					}
					else if ((m_lastUpdateTime - action.LastCallTime).ToMilliSecondsInt() >= action.Delay)
					{
						action.Execute(this);
					}
				}
			}
		}

		#endregion

		protected override UpdateType GetCreationUpdateType(UpdateFieldFlags relation)
		{
			if (relation.HasAnyFlag(UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly))
			{
				return UpdateType.CreateSelf;
			}
			return UpdateType.Create;
		}

		public override void RequestUpdate()
		{
			m_requiresUpdate = true;
		}

		/// <summary>
		/// The current <see cref="IContextHandler"/> of this Character.
		/// </summary>
		public IContextHandler ContextHandler
		{
			get { return m_Map; }
		}

		/// <summary>
		/// Whether this object is in the world and within the current
		/// execution context.
		/// </summary>
		public bool IsInContext
		{
			get
			{
				if (IsInWorld)
				{
					var context = ContextHandler;	// thread-safe
					if (context != null && context.IsInContext)
					{
						return true;
					}
				}
				return false;
			}
		}

		public void EnsureContext()
		{
			if (IsInWorld)
			{
				var handler = ContextHandler;
				if (handler == null)
				{
					//throw new InvalidOperationException("Could not ensure Context because ContextHandler was not set for: " + this);
				}
				else
				{
					handler.EnsureContext();
				}
			}
		}
	}
}