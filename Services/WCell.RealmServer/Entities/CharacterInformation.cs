//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using WCell.RealmServer.Global;
//using WCell.RealmServer.Instances;

//namespace WCell.RealmServer.Entities
//{
//    /// <summary>
//    /// Contains all information of a Character that persists
//    /// after logging out.
//    /// </summary>
//    public class CharacterInformation : IInstanceHolderSet
//    {
//        private uint m_characterId;
//        private Character m_Chr;

//        protected InstanceLog m_InstanceLog;

//        public CharacterInformation(Character chr)
//        {
//            m_Chr = chr;
//            m_characterId = chr.EntityId.Low;
//        }

//        /// <summary>
//        /// EntityId.Low of the represented Character
//        /// </summary>
//        public uint CharacterId
//        {
//            get { return m_characterId; }
//        }

//        public Character Character
//        {
//            get { return m_Chr; }
//            internal set { m_Chr = value; }
//        }

//        public bool HasInstanceCollection
//        {
//            get
//            {
//                return m_InstanceLog != null;
//            }
//        }

//        /// <summary>
//        /// Auto-created if not already existing
//        /// </summary>
//        public InstanceLog InstanceLog
//        {
//            get
//            {
//                if (m_InstanceLog == null)
//                {
//                    m_InstanceLog = new InstanceLog(this);
//                }
//                return m_InstanceLog;
//            }
//            set
//            {
//                m_InstanceLog = value;
//            }
//        }

//        #region Implementation of IInstanceHolder

//        public Character InstanceLeader
//        {
//            get { return this; }
//        }

//        public InstanceLog InstanceLeaderLog
//        {
//            get { return InstanceLog; }
//        }

//        public void ForeachInstanceLog(Action<InstanceLog> callback)
//        {
//            callback(InstanceLog);
//        }

//        public BaseInstance GetActiveInstance(MapInfo map)
//        {
//            if (m_map.Id == map.Id)
//            {
//                return m_map as BaseInstance;
//            }
//            return m_InstanceLog != null ? m_InstanceLog.GetActiveInstance(map) : null;
//        }

//        #endregion
//    }
//}