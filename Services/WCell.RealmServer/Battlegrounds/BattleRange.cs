using System.Collections.Generic;
using WCell.Constants;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Battlegrounds
{
    /// <summary>
    /// A level group queue for battlegrounds (Ex: levels 10 - 19 battleground)
    /// </summary>
    public class BattleRange
    {
        private List<Battlegroup> m_queue; //Our children
        private uint m_battlegroupId;
        private uint m_size;
        private uint m_minLevel;
        private uint m_maxLevel;
        private BattlegroundQueue m_bgqueue; //Our parent
        private Dictionary<uint, Battleground> m_battlegrounds;
		// TODO: Do *not* use too many ReaderWriterLock! They need huge amount of memories and go deep into the kern
        //private ReaderWriterLockSlim m_syncLock;

        public BattleRange(BattlegroundQueue bgqueue, uint battlegroupId)
        {
			// TODO: Fix
			//m_minLevel = bgqueue.Template.MinLevel + ((bgqueue.Template.LevelRange + 1) * battlegroupId);
			//if (m_minLevel > bgqueue.Template.MaxLevel)
			//    m_minLevel = bgqueue.Template.MaxLevel;

			//m_maxLevel = bgqueue.Template.MinLevel + ((bgqueue.Template.LevelRange + 1) * battlegroupId) + bgqueue.Template.LevelRange;
			//if (m_maxLevel > bgqueue.Template.MaxLevel)
			//    m_maxLevel = bgqueue.Template.MaxLevel;

			//m_size = 0;
			//m_ids = new IdQueue();
			//m_battlegrounds = new Dictionary<uint, Battleground>();
			//m_bgqueue = bgqueue;
			//m_battlegroupId = battlegroupId;
			//m_syncLock = new ReaderWriterLockSlim();
        }

        public void Add(Character character)
        {
            Battlegroup bg = new Battlegroup(character, this);
            Add(bg);
        }

        public void Add(Group group)
        {
            Battlegroup bg = new Battlegroup(group, this);
            Add(bg);
        }

        public void Add(Battlegroup battlegroup)
        {
            m_queue.Add(battlegroup);
            m_size += (uint)battlegroup.Size;
        }

        public void Remove(Battlegroup battlegroup)
        {
            m_queue.Remove(battlegroup);
            m_size -= (uint)battlegroup.Size;
        }

        #region Handler
        /// <summary>
        /// Sends the packet to show the battleground window
        /// </summary>
        /// <param name="client"></param>
        /// <param name="speaker"></param>
        /// <param name="character"></param>
        /// <returns></returns>
        public bool SendBattlegroundWindow(IRealmClient client, NPC speaker, Character character)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_BATTLEFIELD_LIST))
            {
                packet.WriteULong(speaker != null ? speaker.EntityId.Full : 0);
                packet.WriteUInt((uint)m_bgqueue.Template.BgID);
                packet.WriteByte(m_battlegroupId); //Battle group
				// TODO: Add sync'ing?
                //m_syncLock.EnterReadLock();
                try
                {
                    packet.WriteUInt(m_battlegrounds.Count); //Count

                    foreach (var bg in m_battlegrounds.Values)
                    {
                        packet.WriteUInt(bg.InstanceId);
                    }
                }
                finally
                {
                    //m_syncLock.ExitReadLock();
                }
                client.Send(packet);
                return true;
            }
        }
        #endregion
    }
}