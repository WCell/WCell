using WCell.Constants;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;
using System;
using WCell.Core.Timers;

namespace WCell.RealmServer.Instances
{
	/// <summary>
	/// Represents an ingame Dungeon Instance for small parties.
	/// </summary>
	public abstract class DungeonInstance : BaseInstance, IUpdatable
	{
		public static float DefaultInstanceTimeoutMinutes = 30 * 60f;
		private DateTime m_lastReset;
		protected TimerEntry m_timeoutTimer;

		public DungeonInstance()
		{
		}

		protected internal override void InitRegion()
		{
			base.InitRegion();

			m_lastReset = DateTime.Now;
			m_timeoutTimer = new TimerEntry(OnTimeout);

			RegisterUpdatableLater(this); 
		}

		public float TimeoutDelay
		{
			get { return DefaultInstanceTimeoutMinutes; }
		}

        /// <summary>
        /// The last time this Instance was reset
        /// </summary>
		public DateTime LastReset
		{
			get { return m_lastReset; }
		}

		/// <summary>
		/// Whether this instance can be reset
		/// </summary>
		public override bool CanReset(Character chr)
		{
			return (chr.Role.IsStaff || chr == m_owner.InstanceLeader) &&
				PlayerCount == 0;
		}

		public override void Reset()
		{
			base.Reset();
			m_lastReset = DateTime.Now;
		}

		protected override void OnEnter(Character chr)
		{
			base.OnEnter(chr);

			if (m_timeoutTimer.IsRunning)
			{
				m_timeoutTimer.Stop();
				s_log.Debug("{0} #{1} timeout timer stopped by: {2}", Name, m_InstanceId, chr.Name);
			}

			if (!chr.Role.IsStaff && Difficulty.BindingType == BindingType.Soft)
			{
				Bind(chr);
			}
		}

		protected void Bind(IInstanceHolderSet holder)
		{
            if (holder.InstanceLeader.Group != null)
            {
                holder.InstanceLeader.Group.ForeachCharacter((chr) => {
					var instances = chr.Instances;
                    if (instances != null)
                    {
						instances.BindTo(this);
                    }
                });
            }
            else
            {
                holder.InstanceLeaderCollection.BindTo(this);
            }
		}

		protected override void OnLeave(Character chr)
		{
		    if (PlayerCount > 1) return;
		    
            if (TimeoutDelay > 0)
		    {
		        m_timeoutTimer.RemainingInitialDelay = TimeoutDelay;
		        m_timeoutTimer.Start();
		    }
		    s_log.Debug("{0} #{1} timeout timer started.", Name, m_InstanceId);
		}

		/// <summary>
		/// Whether the given Character may enter this Instance.
		/// Checked for on Login.
		/// </summary>
		/// <param name="chr"></param>
		/// <returns></returns>
		public override bool CanEnter(Character chr)
		{
			return (chr.LastLogout > m_lastReset) && base.CanEnter(chr);
		}

		#region IUpdatable Members

		public void Update(float dt)
		{
			if (m_timeoutTimer.IsRunning)
			{
				m_timeoutTimer.Update(dt);
			}
		}

		#endregion
	}
}