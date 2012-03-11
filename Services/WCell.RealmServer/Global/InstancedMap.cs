using System;
using WCell.Constants;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Formulas;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Global
{
    public class InstancedMap : Map
    {
        internal protected uint m_InstanceId;

        protected DateTime m_creationTime;

        protected InstancedMap()
        {
            m_creationTime = DateTime.Now;
            XpCalculator = XpGenerator.CalcDefaultXp;
        }

        protected internal override void InitMap()
        {
            m_InstanceId = m_MapTemplate.NextId();

            base.InitMap();
        }

        #region Properties

        /// <summary>
        /// The instances unique identifier, raid and instance IDs are seperate
        /// </summary>
        public override uint InstanceId
        {
            get { return m_InstanceId; }
        }

        /// <summary>
        /// Whether this Instance is active.
        /// Once the last boss has been killed, an instance turns inactive.
        /// </summary>
        public bool IsActive
        {
            get;
            set;
        }

        /// <summary>
        /// Instances are stopped manually
        /// </summary>
        public override bool ShouldStop
        {
            get { return false; }
        }

        /// <summary>
        /// If its a saving type instance, raid or heroic
        /// </summary>
        public bool IsRaid
        {
            get { return m_MapTemplate.Type == MapType.Raid; }
        }

        /// <summary>
        /// If its a PVP area, BattleGround or Arena
        /// </summary>
        public bool IsPVPArea
        {
            get { return m_MapTemplate.Type == MapType.Battleground || m_MapTemplate.Type == MapType.Arena; }
        }

        public override DateTime CreationTime
        {
            get { return m_creationTime; }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        ///
        /// </summary>
        /// <param name="chr"></param>
        /// <returns></returns>
        public virtual bool CanReset(Character chr)
        {
            return chr.Role.IsStaff;
        }

        protected virtual void OnTimeout(int timeElapsed)
        {
            s_log.Debug("{0} #{1} timeout.", Name, m_InstanceId);
            Delete();
        }

        /// <summary>
        /// Teleports player into an instance
        /// </summary>
        /// <param name="chr"></param>
        public virtual void TeleportInside(Character chr)
        {
            TeleportInside(chr, 0);
        }

        public void TeleportInside(Character chr, int entrance)
        {
            if ((uint)entrance >= m_MapTemplate.EntrancePositions.Length)
            {
                entrance = 0;
            }

            TeleportInside(chr, m_MapTemplate.EntrancePositions[entrance]);
        }

        public void TeleportInside(Character chr, Vector3 pos)
        {
            chr.TeleportTo(this, ref pos);
            chr.SendSystemMessage("Welcome to {0} #{1} (created at {2})", Name, InstanceId, m_creationTime.ToString());
        }

        /*
        public void SetOffline(Character character)
        {
            //if (m_occupants.ContainsKey(character.EntityInstanceId.Low))
            //    m_occupants[character.EntityInstanceId.Low] = null;
        }
        */

        public override void RemoveAll()
        {
            base.RemoveAll();
            foreach (var chr in m_characters)
            {
                TeleportOutside(chr);
            }
        }

        public virtual void Delete()
        {
            if (!IsRunning)
            {
                DeleteNow();
            }
            else
            {
                AddMessage(DeleteNow);
            }
        }

        public virtual void DeleteNow()
        {
            EnsureContext();
            EnsureNotUpdating();

            Stop();
            RemoveAll();
            IsDisposed = true;
            m_MapTemplate.RecycleId(m_InstanceId);
        }

        #endregion Methods
    }
}