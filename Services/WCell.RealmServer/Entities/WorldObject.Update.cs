using System;
using System.Collections.Generic;
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


		internal protected virtual void OnEnterRegion()
		{
		}

		internal protected virtual void OnLeavingRegion()
		{
		}

		/// <summary>
		/// The queue of messages. Messages are executed on every region tick.
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
			var ticks = millis / m_region.UpdateDelay;
			var action = new OneShotUpdateObjectAction(ticks, callback);
			CallPeriodically(action);
			return action;
		}

		public OneShotUpdateObjectAction CallDelayedTicks(int ticks, Action<WorldObject> callback)
		{
			var action = new OneShotUpdateObjectAction(ticks, callback);
			CallPeriodically(action);
			return action;
		}

		/// <summary>
		/// Adds a new Action to the list of Actions to be executed every millis.
		/// </summary>
		/// <param name="callback"></param>
		public IUpdateObjectAction CallPeriodically(int millis, Action<WorldObject> callback)
		{
			var ticks = millis / m_region.UpdateDelay;
			var action = new SimpleObjectUpdateAction(ticks, callback);
			CallPeriodically(action);
			return action;
		}

		/// <summary>
		/// Adds a new Action to the list of Actions to be executed every ticks Region-Ticks.
		/// </summary>
		/// <param name="callback"></param>
		public IUpdateObjectAction CallPeriodicallyTicks(int ticks, Action<WorldObject> callback)
		{
			var action = new SimpleObjectUpdateAction(ticks, callback);
			CallPeriodically(action);
			return action;
		}

		/// <summary>
		/// Adds a new Action to the list of Actions to be executed every action.Ticks Region-Ticks.
		/// </summary>
		public void CallPeriodically(IUpdateObjectAction action)
		{
			if (m_updateActions == null)
			{
				m_updateActions = new List<IUpdateObjectAction>(3);
			}
			m_updateActions.Add(action);
		}

		/// <summary>
		/// Removes the given Action
		/// </summary>
		/// <param name="action"></param>
		public void RemoveUpdateAction(IUpdateObjectAction action)
		{
			if (m_updateActions != null)
			{
				if (m_updateActions.Remove(action) 
					//&& m_updateActions.Count == 0
					)
				{
					//m_updateActions = null;
				}
			}
		}

		/// <summary>
		/// Make sure to call this before updating anything else (required for reseting UpdateInfo)
		/// </summary>
		public virtual void Update(float dt)
		{
			m_ticks++;

			IMessage msg;
			while (m_messageQueue.TryDequeue(out msg))
			{
				msg.Execute();
			}

			if (m_areaAura != null)
			{
				m_areaAura.Update(dt);
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
			get { return m_region; }
		}

		public bool IsInContext
		{
			get { return ContextHandler != null && ContextHandler.IsInContext; }
		}

		public void EnsureContext()
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
