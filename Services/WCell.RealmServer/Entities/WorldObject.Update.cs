using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Util.Collections;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.Util.Threading;
using WCell.Core.Timers;

namespace WCell.RealmServer.Entities
{
	public interface IUpdateObjectAction
	{
		Action<WorldObject> Callback { get; }

		int Ticks { get; }
	}

	public class SimpleObjectUpdateAction : IUpdateObjectAction
	{
		public SimpleObjectUpdateAction() { }

		public SimpleObjectUpdateAction(int ticks, Action<WorldObject> callback)
		{
			Callback = callback;
			Ticks = ticks;
		}

		public Action<WorldObject> Callback
		{
			get;
			set;
		}

		public int Ticks
		{
			get;
			set;
		}

		public override bool Equals(object obj)
		{
			return obj is SimpleObjectUpdateAction && Callback == ((SimpleObjectUpdateAction)obj).Callback;
		}

		public override int GetHashCode()
		{
			return Callback.GetHashCode();
		}
	}

	public class OneShotUpdateObjectAction : SimpleObjectUpdateAction
	{
		public OneShotUpdateObjectAction(int ticks, Action<WorldObject> callback)
		{
			Ticks = ticks;
			Callback = obj => {
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
		int m_ticks;

		protected List<IUpdateObjectAction> m_updateActions;


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

		#region Object Updating
		public override UpdatePriority UpdatePriority
		{
			get { return m_UpdatePriority; }
		}

		public void SetUpdatePriority(UpdatePriority priority)
		{
			m_UpdatePriority = priority;
		}

		public OneShotUpdateObjectAction CallDelayed(int millis, Action<WorldObject> callback)
		{
			var ticks = millis / m_Map.UpdateDelay;
			var action = new OneShotUpdateObjectAction(ticks, callback);
			AddUpdateAction(action);
			return action;
		}

		public OneShotUpdateObjectAction CallDelayedTicks(int ticks, Action<WorldObject> callback)
		{
			var action = new OneShotUpdateObjectAction(ticks, callback);
			AddUpdateAction(action);
			return action;
		}

		/// <summary>
		/// Adds a new Action to the list of Actions to be executed every millis.
		/// </summary>
		/// <param name="callback"></param>
		public IUpdateObjectAction CallPeriodically(int millis, Action<WorldObject> callback)
		{
			var ticks = millis / m_Map.UpdateDelay;
			var action = new SimpleObjectUpdateAction(ticks, callback);
			AddUpdateAction(action);
			return action;
		}

		/// <summary>
		/// Adds a new Action to the list of Actions to be executed every ticks Map-Ticks.
		/// </summary>
		/// <param name="callback"></param>
		public IUpdateObjectAction CallPeriodicallyTicks(int ticks, Action<WorldObject> callback)
		{
			var action = new SimpleObjectUpdateAction(ticks, callback);
			AddUpdateAction(action);
			return action;
		}

		/// <summary>
		/// Adds a new Action to the list of Actions to be executed every action.Ticks Map-Ticks.
		/// </summary>
		public void AddUpdateAction(IUpdateObjectAction action)
		{
			if (m_updateActions == null)
			{
				m_updateActions = new List<IUpdateObjectAction>(3);
			}
			m_updateActions.Add(action);
		}

		public bool HasUpdateAction(Func<IUpdateObjectAction, bool> predicate)
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
		/// <param name="action"></param>
		public bool RemoveUpdateAction(IUpdateObjectAction action)
		{
			if (m_updateActions != null)
			{
				if (m_updateActions.Remove(action))
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
			m_ticks++;

			IMessage msg;
			while (m_messageQueue.TryDequeue(out msg))
			{
				msg.Execute();
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
				for (var i = count-1; i >= 0; i--)
				{
					var action = m_updateActions[i];
					if (action.Ticks == 0)
					{
						// Keep an eye out for this...
						action.Callback(this);
					}
					else if (CheckTicks(action.Ticks))
					{
						action.Callback(this);
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