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
	public abstract class AIGroup
	{
		private Unit m_Leader;
		private List<NPC> m_Mobs;

		public AIGroup(Unit leader, List<NPC> mobs)
		{
			m_Leader = leader;
			m_Mobs = mobs;
		}

		public Unit Leader
		{
			get { return m_Leader; }
		}

		public List<NPC> Mobs
		{
			get { return m_Mobs; }
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
			m_Mobs.Add(npc);
			npc.Group = this;
		}

		public void RemoveMob(NPC npc)
		{
			if (m_Mobs.Remove(npc))
			{
				npc.Group = null;
			}
		}

		public void Aggro(Unit unit)
		{
			for (var i = 0; i < m_Mobs.Count; i++)
			{
				var mob = m_Mobs[i];
				mob.ThreatCollection.AddNew(unit);
			}
		}
	}
}