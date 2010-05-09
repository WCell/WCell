using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Talents;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using NHibernate.Criterion;
using Castle.ActiveRecord;

namespace WCell.RealmServer.Talents
{
    public class SpecProfile
    {
        public static int MAX_TALENT_GROUPS = 2;
        public static int MAX_GLYPHS_PER_GROUP = 6;

        private Character owner;
        private List<ITalentRecord>[] talentGroups;
        private List<GlyphRecord>[] glyphGroups;
        private List<ActionbarRecord>[] actionbarGroups;

        private SpecProfile()
        {
        }

        /// <summary>
        /// Creates a new SpecProfile and saves it to the db.
        /// </summary>
        /// <param name="record">The character or pet that will own the spec profile.</param>
        /// <returns>The newly created SpecProfile.</returns>
        public static SpecProfile NewSpecProfile(Character owner)
        {
            var newProfile = new SpecProfile() {
				owner = owner,
                talentGroupCount = 1,
                TalentGroupInUse = 0,
                glyphGroupRecords = new List<GlyphRecord>(MAX_GLYPHS_PER_GROUP*MAX_TALENT_GROUPS),
                actionbarGroupRecords = new List<ActionbarRecord>()
            };

			// TODO: Load existing profiles
            //newProfile.CreateAndFlush();

            for (var i = 0; i < MAX_GLYPHS_PER_GROUP; i++)
            {
                newProfile.glyphGroupRecords.Add(GlyphRecord.NewGlyphRecord(newProfile, 0));
            }
            
            return newProfile;
		}

		[PrimaryKey(PrimaryKeyType.Foreign)]
		private long id
		{
			get;
			set;
		}

		[Field(NotNull = true)]
		private int talentGroupCount;

    	private int m_TalentGroupInUse = 0;

    	/// <summary>
		/// The Id of the Talent Group currently in use.
		/// Currently 0 or 1.
		/// </summary>
		[Property]
		public int TalentGroupInUse
    	{
    		get { return m_TalentGroupInUse; }
    		set { m_TalentGroupInUse = value; }
    	}

    	private IList<GlyphRecord> glyphGroupRecords
		{
			get;
			set;
		}

		private IList<ActionbarRecord> actionbarGroupRecords
		{
			get;
			set;
		}

		[Property]
		public DateTime? LastModifiedOn
		{
			get;
			set;
		}


        /// <summary>
        /// The number of TalentGroups this IHasTalents can store.
        /// Currently 1 or 2.
        /// </summary>
        public int TalentGroupCount
        {
            get { return talentGroupCount; }
            set
            {
                if (value < 1) value = 1;
                if (value > MAX_TALENT_GROUPS) value = MAX_TALENT_GROUPS;
                talentGroupCount = value;

                ReSizeTalentGroupList(value);
            }
        }

        public List<ITalentRecord>[] TalentGroups
        {
            get
            {
                if (talentGroups == null)
                {
                    talentGroups = new List<ITalentRecord>[MAX_TALENT_GROUPS];
                    for (var i = 0; i < TalentGroupCount; i++)
                    {
                        talentGroups[i] = new List<ITalentRecord>();
                    }
					// TODO: Get all Talent records
                    //SortTalentGroups();
                }
                return talentGroups;
            }
            set
            {
                talentGroups = value;
            }
        }

        public List<GlyphRecord>[] GlyphGroups
        {
            get
            {
                if (glyphGroups == null)
                {
                    glyphGroups = new List<GlyphRecord>[MAX_TALENT_GROUPS];
                    for (var i = 0; i < TalentGroupCount; i++)
                    {
                        glyphGroups[i] = new List<GlyphRecord>(MAX_GLYPHS_PER_GROUP);
                    }
                    SortGlyphGroups();
                }
                return glyphGroups;
            }
        }

        public List<ActionbarRecord>[] ActionbarGroups
        {
            get
            {
                if (actionbarGroups == null)
                {
                    actionbarGroups = new List<ActionbarRecord>[MAX_TALENT_GROUPS];
                    SortActionbarGroups();
                }
                return actionbarGroups;
            }
        }

        public void LearnTalent(SimpleTalentDescriptor talent)
        {
            var talentId = talent.TalentId;
            var rank = talent.Rank;

            if (!owner.Talents.Learn(talentId, rank)) return;

            //AddOrUpdateTalentRecord(talentId, rank);

            // Update the client with the new Talent List.
			TalentHandler.SendTalentGroupList(owner);
        }

        public void ApplySpec(int talentGroupId)
        {
            if (talentGroupId >= MAX_TALENT_GROUPS) return;
            if (talentGroupId < (talentGroups.Length - 1)) return;
            if (talentGroups[talentGroupId] == null)
            {
                talentGroups[talentGroupId] = new List<ITalentRecord>();
            }

            // Apply the Talent Spec
			owner.Talents.ApplySpec(TalentGroups[talentGroupId]);
            TalentGroupInUse = talentGroupId;

            // Apply the Glyph Spec
            // Apply the Actionbar Spec
        }

		//public void LearnTalentGroupTalents(List<SimpleTalentDescriptor> newTalents)
		//{
		//    var groupId = CycleTalentGroup();
		//    if (talentGroups[groupId] == null)
		//    {
		//        talentGroups[groupId] = new List<ITalentRecord>(newTalents.Count);
		//    }

		//    foreach (var talent in newTalents)
		//    {
		//        AddOrUpdateTalentRecord(talent);
		//    }

		//    // Update Glyph Groups ??
		//    // Update ActionBar Groups ??

		//    ApplySpec(groupId);

		//    TalentHandler.SendTalentGroupList(owner);
		//}

        public void ResetActiveTalentGroup()
        {
            foreach (var record in TalentGroups[TalentGroupInUse])
            {
                //DeleteTalentRecord(record);
            }
            TalentGroups[TalentGroupInUse] = new List<ITalentRecord>();

			owner.Talents.ResetAll();

            // Report the reset to the client.
			TalentHandler.SendTalentGroupList(owner);
        }

		// Unnecessary -> Use SpellRecord objects instead.
		//private void AddOrUpdateTalentRecord(TalentId talentId, int rank)
		//{
		//    var found = false;
		//    // See if we already have an entry for this Talent
		//    foreach (var record in TalentGroups[TalentGroupInUse])
		//    {
		//        if (record.TalentId != talentId) continue;
		//        if (record.Rank != rank)
		//        {
		//            record.Rank = rank;
		//            //record.UpdateAndFlush();
		//        }
		//        found = true;
		//        break;
		//    }

		//    if (!found)
		//    {
		//        AddNewTalentRecord(talentId, rank);
		//    }
		//}

		//private void AddNewTalentRecord(TalentId talentId, int rank)
		//{
		//    var newRecord = ITalentRecord.NewTalentRecord(this, TalentGroupInUse, talentId, rank);
		//    TalentGroups[TalentGroupInUse].Add(newRecord);
		//    talentGroupRecords.Add(newRecord);
		//}

        private void SortGlyphGroups()
        {
            var count = 0;
            foreach (var record in glyphGroupRecords)
            {
                if (count >= MAX_GLYPHS_PER_GROUP) break;

                var glyphGroupId = record.GlyphGroupId;
                if (glyphGroupId >= MAX_TALENT_GROUPS) continue;
                
                glyphGroups[glyphGroupId].Add(record);
                count++;
            }
        }

        private void SortActionbarGroups()
        {
            foreach (var record in actionbarGroupRecords)
            {
                var actionbarGroupId = record.ActionbarGroupId;
                if (actionbarGroupId < MAX_TALENT_GROUPS)
                {
                    actionbarGroups[actionbarGroupId].Add(record);
                }
            }
        }

        private int CycleTalentGroup()
        {
            return TalentGroupInUse > 0 ? 0 : 1;
        }

        private void ReSizeTalentGroupList(int newSize)
        {
            if (newSize < 1) newSize = 1;

            // We are reducing the size of the TalentGroupList.
            if (newSize < talentGroups.Length)
            {
                // Is the TalentGroup in use slated for removal?
                if (TalentGroupInUse > (newSize - 1))
                {
                    // We need to move this alternate spec to the main spec so as to not loose it
                    // The Main spec is lost of course.
                    talentGroups[0] = talentGroups[TalentGroupInUse];
                    TalentGroupInUse = 0;
                }
            }

            // Reset the TalentGroup Lists to refelct the new/removed talentGroups
            var newList = new List<ITalentRecord>[newSize];
            for (var i = 0; i < newSize; i++)
            {
                if (i < talentGroups.Length)
                {
                    newList[i] = talentGroups[i];
                    continue;
                }
                newList[i] = new List<ITalentRecord>();
            }
            TalentGroups = newList;

			TalentHandler.SendTalentGroupList(owner);
        }
    }
}
