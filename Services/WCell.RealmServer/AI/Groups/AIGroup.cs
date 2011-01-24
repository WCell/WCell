using System;
using System.Collections;
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
	public class AIGroup : IList<NPC>
	{
		private NPC m_Leader;
		private List<NPC> groupList;

		public AIGroup()
		{
			groupList = new List<NPC>();
		}

		public AIGroup(NPC leader) : this()
		{
			m_Leader = leader;
			if (leader != null && !Contains(leader))
			{
				Add(leader);
			}
		}

		public AIGroup(IEnumerable<NPC> mobs)
		{
			groupList = new List<NPC>(mobs);
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

		#region Implementation of IEnumerable

		public IEnumerator<NPC> GetEnumerator()
		{
			return groupList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Implementation of ICollection<NPC>

		public void Add(NPC item)
		{
			groupList.Add(item);
		}

		public void Clear()
		{
			groupList.Clear();
		}

		public bool Contains(NPC item)
		{
			return groupList.Contains(item);
		}

		void ICollection<NPC>.CopyTo(NPC[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public bool Remove(NPC item)
		{
			return groupList.Remove(item);
		}

		public int Count
		{
			get { return groupList.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		#endregion

		#region Implementation of IList<NPC>

		public int IndexOf(NPC item)
		{
			return groupList.IndexOf(item);
		}

		void IList<NPC>.Insert(int index, NPC item)
		{
			throw new NotImplementedException();
		}

		public void RemoveAt(int index)
		{
			groupList.RemoveAt(index);
		}

		public NPC this[int index]
		{
			get { return groupList[index]; }
			set { groupList[index] = value; }
		}

		#endregion
	}
}