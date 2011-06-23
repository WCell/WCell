using System;
using System.Threading;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Core.Timers;
using WCell.RealmServer.Battlegrounds;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.Global;
using WCell.RealmServer.Network;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.Instances
{
	public delegate void InstanceDelegate(BaseInstance instance);

	/// <summary>
	/// The base class for all WoW-style "Instances" (Dungeon, Heroic, Raid etc)
	/// 
	/// TODO:
	/// - SMSG_INSTANCE_RESET_FAILURE: The party leader has attempted to reset the instance you are in. Please zone out to allow the instance to reset.
	/// </summary>
	public abstract class BaseInstance : InstancedMap, IUpdatable
	{
		/// <summary>
		/// The timeout for normal Dungeon instances
		/// </summary>
		public static int DefaultInstanceTimeoutMillis = 30 * 60 * 1000;

		private IInstanceHolderSet m_owner;
		private DateTime m_expiryTime;
		internal MapDifficultyEntry difficulty;
		private DateTime m_lastReset;
		private TimerEntry m_timeoutTimer;
		private InstanceProgress progress;

		protected BaseInstance()
		{
		}

		protected internal override void InitMap()
		{
			base.InitMap();

			var secs = difficulty.ResetTime;
			if (secs > 0)
			{
				// TODO: Set expiry time correctly
				//m_expiryTime = InstanceMgr.
			}

			m_lastReset = DateTime.Now;
			
			m_timeoutTimer = new TimerEntry(OnTimeout);

			RegisterUpdatableLater(this); 
		}

		/// <summary>
		/// Whether this instance will ever expire
		/// </summary>
		public bool CanExpire
		{
			get { return m_expiryTime != default(DateTime); }
		}

		public DateTime ExpiryTime
		{
			get { return m_expiryTime; }
		}

		/// <summary>
		/// Difficulty of the instance
		/// </summary>
		public override MapDifficultyEntry Difficulty
		{
			get { return difficulty; }
		}

		public IInstanceHolderSet Owner
		{
			get { return m_owner; }
			set { m_owner = value; }
		}
		
		public int TimeoutDelay
		{
			get { return DefaultInstanceTimeoutMillis; }
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
				m_timeoutTimer.Start(TimeoutDelay, 0);
		    }
		    s_log.Debug("{0} #{1} timeout timer started.", Name, m_InstanceId);
		}

		#region IUpdatable Members

		public void Update(int dt)
		{
			m_timeoutTimer.Update(dt);
		}

		#endregion

		public override bool CanEnter(Character chr)
		{
			if (chr.LastLogout > m_lastReset)
			{
				
			}
			if (base.CanEnter(chr))
			{
				if (Owner == null) return true;

				var leader = Owner.InstanceLeader;
				return (leader != null && chr.IsAlliedWith(leader)) || chr.GodMode;
			}
			return false;
		}

		public override void TeleportOutside(Character chr)
		{
			chr.TeleportToNearestGraveyard();
		}

		#region Cleanup & Misc
		public override void DeleteNow()
		{
			InstanceMgr.Instances.RemoveInstance(MapId, InstanceId);
			base.DeleteNow();
		}

		protected override void Dispose()
		{
			base.Dispose();
			m_owner = null;
		}

		public override string ToString()
		{
			var ownerStr = "";
			if (Owner != null && Owner.InstanceLeader != null)
			{
				ownerStr = " - Owned by: " + Owner.InstanceLeader.Name;
			}
			return base.ToString() + ((difficulty.IsHeroic ? " [Heroic]" : "") + ownerStr);
		}

		public override sealed void Save()
		{
			if (progress == null)
			{
				progress = new InstanceProgress(MapId, InstanceId);
			}
			PerformSave();
			progress.Save();
		}

		/// <summary>
		/// Method is to be overridden by instance implementation
		/// </summary>
		protected virtual void PerformSave()
		{
		}
		#endregion
	}
}