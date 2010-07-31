//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using WCell.RealmServer.Database;
//using WCell.RealmServer.Entities;
//using NHibernate.Criterion;

//namespace WCell.RealmServer.Talents
//{
//    public class CharacterSpecProfile : TalentSpecProfile
//    {
//        protected List<GlyphRecord>[] glyphGroups;
//        private IList<GlyphRecord> m_glyphGroupRecords;

//        private byte[] m_actionBar;

//        public CharacterSpecProfile(Character owner) : base(owner)
//        {
//        }

//        public Character CharOwner
//        {
//            get { return (Character)m_owner; }
//        }

//        [Property(ColumnType = "BinaryBlob")]
//        public byte[] ActionBar
//        {
//            get { return m_actionBar; }
//            set { m_actionBar = value; }
//        }

//        /// <summary>
//        /// Creates a new TalentSpecProfile and saves it to the db.
//        /// </summary>
//        /// <param name="record">The character or pet that will own the spec profile.</param>
//        /// <returns>The newly created TalentSpecProfile.</returns>
//        public static TalentSpecProfile NewSpecProfile(Character owner)
//        {
//            var newProfile = new CharacterSpecProfile(owner)
//            {
//                talentGroupCount = 1,
//                UsedTalentSpecId = 0,
//                m_currentSpec = new TalentSpec(),
//                m_glyphGroupRecords = new List<GlyphRecord>(MAX_GLYPHS_PER_GROUP * MAX_TALENT_GROUPS),
//                actionbarGroupRecords = new List<ActionbarRecord>()
//            };

//            // TODO: Load existing profiles
//            //newProfile.CreateAndFlush();

//            for (var i = 0; i < MAX_GLYPHS_PER_GROUP; i++)
//            {
//                newProfile.m_glyphGroupRecords.Add(GlyphRecord.NewGlyphRecord(newProfile, 0));
//            }

//            return newProfile;
//        }

//        public override int FreeTalentPoints
//        {
//            get { return ((Character) m_owner).FreeTalentPoints; }
//            set { ((Character)m_owner).FreeTalentPoints = value; }
//        }

//        public List<GlyphRecord>[] GlyphGroups
//        {
//            get
//            {
//                if (glyphGroups == null)
//                {
//                    glyphGroups = new List<GlyphRecord>[MAX_TALENT_GROUPS];
//                    for (var i = 0; i < TalentGroupCount; i++)
//                    {
//                        glyphGroups[i] = new List<GlyphRecord>(MAX_GLYPHS_PER_GROUP);
//                    }
//                    SortGlyphGroups();
//                }
//                return glyphGroups;
//            }
//        }

//        public List<ActionbarRecord>[] ActionbarGroups
//        {
//            get
//            {
//                if (actionbarGroups == null)
//                {
//                    actionbarGroups = new List<ActionbarRecord>[MAX_TALENT_GROUPS];
//                    SortActionbarGroups();
//                }
//                return actionbarGroups;
//            }
//        }

//        private void SortGlyphGroups()
//        {
//            var count = 0;
//            foreach (var record in m_glyphGroupRecords)
//            {
//                if (count >= MAX_GLYPHS_PER_GROUP) break;

//                var glyphGroupId = record.GlyphGroupId;
//                if (glyphGroupId >= MAX_TALENT_GROUPS) continue;

//                glyphGroups[glyphGroupId].Add(record);
//                count++;
//            }
//        }

//        private void SortActionbarGroups()
//        {
//            foreach (var record in actionbarGroupRecords)
//            {
//                var actionbarGroupId = record.ActionbarGroupId;
//                if (actionbarGroupId < MAX_TALENT_GROUPS)
//                {
//                    actionbarGroups[actionbarGroupId].Add(record);
//                }
//            }
//        }
//    }
//}