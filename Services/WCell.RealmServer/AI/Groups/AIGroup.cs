using System;
using System.Collections;
using System.Collections.Generic;
using WCell.Constants.Updates;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Groups
{
    /// <summary>
    ///
    /// </summary>
    public class AIGroup : IList<NPC>
    {
        private NPC m_Leader;
        private readonly List<NPC> groupList;

        public AIGroup()
        {
            groupList = new List<NPC>();
        }

        public AIGroup(NPC leader)
            : this()
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
            set
            {
                m_Leader = value;
                if (value != null && !Contains(value))
                {
                    Add(value);
                }
            }
        }

        public virtual BrainState DefaultState
        {
            get { return BaseBrain.DefaultBrainState; }
        }

        public virtual UpdatePriority UpdatePriority
        {
            get { return UpdatePriority.Background; }
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

        #endregion Implementation of IEnumerable

        #region Implementation of ICollection<NPC>

        /// <summary>
        /// Adds the given NPC to this group
        /// </summary>
        public void Add(NPC npc)
        {
            if (!npc.IsAlive) return;

            groupList.Add(npc);
            npc.Group = this;
            if (Leader == null)
            {
                m_Leader = npc;
            }
            else if (npc != Leader)
            {
                var mainTarget = Leader.ThreatCollection.CurrentAggressor;
                if (mainTarget != null)
                {
                    // double threat of leader's main target for the new NPC
                    npc.ThreatCollection[mainTarget] = 2 * npc.ThreatCollection[mainTarget] + 1;

                    // generate threat on all other enemies, too
                    foreach (var hostile in m_Leader.ThreatCollection)
                    {
                        npc.ThreatCollection.AddNewIfNotExisted(hostile.Key);
                    }
                }
            }
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
            groupList.CopyTo(array, arrayIndex);
        }

        public bool Remove(NPC npc)
        {
            if (groupList.Remove(npc))
            {
                if (npc == m_Leader)
                {
                    m_Leader = null;
                }
                npc.Group = null;
                return true;
            }
            return false;
        }

        public int Count
        {
            get { return groupList.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        #endregion Implementation of ICollection<NPC>

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

        #endregion Implementation of IList<NPC>
    }
}