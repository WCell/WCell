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
	public abstract class BaseInstance : InstancedMap
	{
		protected IInstanceHolderSet m_owner;
		protected DateTime m_expiryTime;
		protected internal MapDifficultyEntry m_difficulty;

		protected BaseInstance()
		{
		}

		protected internal override void InitMap()
		{
			base.InitMap();

			var secs = m_difficulty.ResetTime;
			if (secs > 0)
			{
				// TODO: Set expiry time correctly
				//m_expiryTime = InstanceMgr.
			}
		}

		public bool Expires
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
			get { return m_difficulty; }
		}

		public IInstanceHolderSet Owner
		{
			get { return m_owner; }
			set { m_owner = value; }
		}

		public override bool CanEnter(Character chr)
		{
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

		public override void DeleteNow()
		{
			World.RemoveInstance(this);
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
			return base.ToString() + ((m_difficulty.IsHeroic ? " [Heroic]" : "") + ownerStr);
		}
	}
}