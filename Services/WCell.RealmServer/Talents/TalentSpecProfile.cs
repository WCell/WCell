//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using WCell.Constants.Talents;
//using WCell.RealmServer.Database;
//using WCell.RealmServer.Entities;
//using WCell.RealmServer.Global;
//using WCell.RealmServer.Handlers;
//using NHibernate.Criterion;
//using Castle.ActiveRecord;
//using WCell.RealmServer.Network;

//namespace WCell.RealmServer.Talents
//{
//    public abstract class TalentSpecProfile
//    {
//        public static int MAX_TALENT_GROUPS = 2;
//        public static int MAX_GLYPHS_PER_GROUP = 6;

//        protected Unit m_owner;
//        protected TalentCollection m_Talents;

//        protected TalentSpec[] m_talentSpecs;
//        protected TalentSpec m_currentSpec;

//        protected int talentGroupCount;

//        protected TalentSpecProfile(Unit owner)
//        {
//            m_owner = owner;
//            m_Talents = new TalentCollection(owner);
//        }

//        public TalentCollection Talents
//        {
//            get { return m_Talents; }
//            internal set { m_Talents = value; }
//        }

//        public abstract int FreeTalentPoints
//        {
//            get;
//            set;
//        }

//        public DateTime? LastTalentResetTime
//        {
//            get;
//            set;
//        }

//        public abstract bool IsPetProfile
//        {
//            get;
//        }

//        public IRealmClient Client
//        {
//            get
//            {
//                if (m_owner is Character)
//                {
//                    return ((Character)m_owner).Client;
//                }
//                var master = m_owner.Master;
//                if (master != null && master is Character)
//                {
//                    return ((Character)master).Client;
//                }
//                return null;
//            }
//        }

//        public void LearnTalent(SimpleTalentDescriptor talent)
//        {
//            var talentId = (TalentId)talent.TalentId;
//            var rank = talent.Rank;

//            if (!m_Talents.Learn(talentId, rank + 1)) return;

//            AddOrUpdateTalentRecord(talentId, rank);

//            // Update the client with the new Talent List.
//            TalentHandler.SendTalentGroupList(this);
//        }

//        public void ApplySpec(int talentGroupId)
//        {
//            if (talentGroupId >= MAX_TALENT_GROUPS) return;
//            if (talentGroupId < (m_talentSpecs.Length - 1)) return;
//            m_currentSpec = m_talentSpecs[talentGroupId];
//            if (m_currentSpec == null)
//            {
//                m_talentSpecs[talentGroupId] = new List<TalentSpec>();
//            }

//            // Apply the Talent Spec
//            m_owner.Talents.ApplySpec(curSpec);
//            UsedTalentSpecId = talentGroupId;

//            // Apply the Glyph Spec
//            // Apply the Actionbar Spec
//        }

//        private void AddOrUpdateTalentRecord(SimpleTalentDescriptor talent)
//        {
//            AddOrUpdateTalentRecord((TalentId)talent.TalentId, talent.Rank);
//        }

//        private void AddOrUpdateTalentRecord(TalentId talentId, int rank)
//        {
//            var found = false;

//            // See if we already have an entry for this Talent
//            foreach (var record in m_currentSpec.Talents)
//            {
//                if (record.TalentId != talentId) continue;
//                if (record.Rank != rank)
//                {
//                    record.Rank = rank;
//                    //record.UpdateAndFlush();
//                }
//                found = true;
//                break;
//            }

//            if (!found)
//            {
//                AddNewTalentRecord(talentId, rank);
//            }
//        }

//        private void AddNewTalentRecord(TalentId talentId, int rank)
//        {
//            var newRecord = TalentRecord.NewTalentRecord(UsedTalentSpecId, talentId, rank);
//            m_currentSpec.Talents.Add(newRecord);
//            m_currentSpec.Talents.Add(newRecord);
//        }

//        private void DeleteTalentRecord(TalentRecord record)
//        {
//            m_currentSpec.Remove(record);
//            //record.Delete();
//        }


//        #region Groups
//        /// <summary>
//        /// The Id of the Talent Group currently in use.
//        /// Currently 0 or 1.
//        /// </summary>
//        public int UsedTalentSpecId
//        {
//            get;
//            set;
//        }


//        /// <summary>
//        /// The number of TalentSpecs this IHasTalents can store.
//        /// Currently 1 or 2.
//        /// </summary>
//        public int TalentGroupCount
//        {
//            get { return talentGroupCount; }
//            set
//            {
//                if (value < 1) value = 1;
//                if (value > MAX_TALENT_GROUPS) value = MAX_TALENT_GROUPS;
//                talentGroupCount = value;

//                ReSizeTalentGroupList(value);
//            }
//        }


//        public TalentSpec[] TalentSpecs
//        {
//            get
//            {
//                if (m_talentSpecs == null)
//                {
//                    m_talentSpecs = new List<TalentRecord>[MAX_TALENT_GROUPS];
//                    for (var i = 0; i < TalentGroupCount; i++)
//                    {
//                        m_talentSpecs[i] = new List<TalentRecord>();
//                    }
//                    SortTalentGroups();
//                }
//                return m_talentSpecs;
//            }
//            set
//            {
//                m_talentSpecs = value;
//            }
//        }

//        private void SortTalentGroups()
//        {
//            foreach (var record in m_currentSpec)
//            {
//                var talentGroupId = record.TalentGroupId;
//                if (talentGroupId < MAX_TALENT_GROUPS)
//                {
//                    m_talentSpecs[talentGroupId].Add(record);
//                }
//            }
//        }

//        private int CycleTalentGroup()
//        {
//            return UsedTalentSpecId > 0 ? 0 : 1;
//        }

//        private void ReSizeTalentGroupList(int newSize)
//        {
//            if (newSize < 1) newSize = 1;

//            // We are reducing the size of the TalentGroupList.
//            if (newSize < m_talentSpecs.Length)
//            {
//                // Is the TalentGroup in use slated for removal?
//                if (UsedTalentSpecId > (newSize - 1))
//                {
//                    // We need to move this alternate spec to the main spec so as to not loose it
//                    // The Main spec is lost of course.
//                    m_talentSpecs[0] = m_talentSpecs[UsedTalentSpecId];
//                    UsedTalentSpecId = 0;
//                }
//            }

//            // Reset the TalentGroup Lists to refelct the new/removed m_talentSpecs
//            var newList = new List<TalentRecord>[newSize];
//            for (var i = 0; i < newSize; i++)
//            {
//                if (i < m_talentSpecs.Length)
//                {
//                    newList[i] = m_talentSpecs[i];
//                    continue;
//                }
//                newList[i] = new List<TalentRecord>();
//            }
//            TalentSpecs = newList;

//            TalentHandler.SendTalentGroupList(m_owner);
//        }


//        public void LearnTalentGroupTalents(List<SimpleTalentDescriptor> newTalents)
//        {
//            var groupId = CycleTalentGroup();
//            if (m_talentSpecs[groupId] == null)
//            {
//                m_talentSpecs[groupId] = new List<TalentRecord>(newTalents.Count);
//            }

//            foreach (var talent in newTalents)
//            {
//                AddOrUpdateTalentRecord(talent);
//            }

//            // Update Glyph Groups ??
//            // Update ActionBar Groups ??

//            ApplySpec(groupId);

//            TalentHandler.SendTalentGroupList(m_owner);
//        }

//        public void ResetActiveTalentGroup()
//        {
//            foreach (var record in TalentSpecs[UsedTalentSpecId])
//            {
//                DeleteTalentRecord(record);
//            }
//            TalentSpecs[UsedTalentSpecId] = new List<TalentRecord>();

//            m_owner.Talents.ResetAll();

//            // Report the reset to the client.
//            TalentHandler.SendTalentGroupList(m_owner);
//        }
//        #endregion
//    }
//}
