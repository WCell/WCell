using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.AI.Brains;

namespace WCell.RealmServer.AI.Groups
{
	/// <summary>
	/// 
	/// </summary>
	public class AIGroup : List<NPC>
	{
		private NPC m_Leader;

		public AIGroup(NPC leader = null)
		{
			m_Leader = leader;
			if (leader != null && !Contains(leader))
			{
				Add(leader);
			}
		}

		public AIGroup(IEnumerable<NPC> mobs)
			: base(mobs)
		{
		}

		public NPC Leader
		{
			get { return m_Leader; }
		}

		public virtual BrainState DefaultState
		{
			get { return BaseBrain.DefaultBrainState; }
		}

		public virtual UpdatePriority UpdatePriority
		{
			get { return UpdatePriority.Background; }
		}

		public void AddMob(NPC npc)
		{
			Add(npc);
			npc.Group = this;
		}

		public void RemoveMob(NPC npc)
		{
			if (Remove(npc))
			{
				npc.Group = null;
			}
		}

		public void Aggro(Unit unit)
		{
			foreach (var mob in this)
			{
				mob.ThreatCollection.AddNewIfNotExisted(unit);
			}
		}
	}
}