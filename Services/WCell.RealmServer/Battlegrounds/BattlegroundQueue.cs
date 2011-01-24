using System.Linq;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;
using WCell.RealmServer.Global;
using WCell.Util.Collections;
using WCell.RealmServer.Misc;
using WCell.Constants.Factions;
using WCell.RealmServer.Groups;
using WCell.Constants;
using WCell.Core;
using System.Collections.Generic;

namespace WCell.RealmServer.Battlegrounds
{
	/// <summary>
	/// A BattlegroundQueue manages access of Players to Battlegrounds
	/// </summary>
	public abstract class BattlegroundQueue
	{
		protected BattlegroundTemplate m_Template;

		protected int m_BracketId;
		protected int m_MinLevel;
		protected int m_MaxLevel;

		public readonly BattlegroundTeamQueue[] TeamQueues =
			new BattlegroundTeamQueue[(int)BattlegroundSide.End];

		protected BattlegroundQueue()
		{
			TeamQueues[(int)BattlegroundSide.Alliance] = CreateTeamQueue(BattlegroundSide.Alliance);
			TeamQueues[(int)BattlegroundSide.Horde] = CreateTeamQueue(BattlegroundSide.Horde);
		}

		public BattlegroundQueue(BattlegroundTemplate template, int bracketId, int minLevel, int maxLevel)
			: this()
		{
			m_Template = template;
			m_BracketId = bracketId;
			m_MinLevel = minLevel;
			m_MaxLevel = maxLevel;
		}

		protected abstract BattlegroundTeamQueue CreateTeamQueue(BattlegroundSide side);

		public int BracketId
		{
			get { return m_BracketId; }
		}

		public int MinLevel
		{
			get { return m_MinLevel; }
		}

		public int MaxLevel
		{
			get { return m_MaxLevel; }
		}

		public BattlegroundTemplate Template
		{
			get { return m_Template; }
		}

		public abstract bool RequiresLocking
		{
			get;
		}

		public BattlegroundTeamQueue GetTeamQueue(Character chr)
		{
			return GetTeamQueue(chr.Faction.Group.GetBattlegroundSide());
		}

		public BattlegroundTeamQueue GetTeamQueue(BattlegroundSide side)
		{
			return TeamQueues[(int)side];
		}

		public uint InstanceId
		{
			get { return Battleground != null ? Battleground.InstanceId : 0; }
		}

		public bool CanEnter(Character chr)
		{
			return chr.Level.IsBetween(MinLevel, MaxLevel) && m_Template.MapTemplate.MayEnter(chr);
		}

		public abstract Battleground Battleground
		{
			get;
		}

		public int AverageWaitTime
		{
			get
			{
				// TODO
				return 5000;
			}
		}

		internal protected virtual void Dispose()
		{
			foreach (var queue in TeamQueues)
			{
				foreach (var request in queue.PendingRequests)
				{
					request.Cancel();
				}
			}

		}
	}
}