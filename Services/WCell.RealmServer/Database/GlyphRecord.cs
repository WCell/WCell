using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.ActiveRecord;
using WCell.RealmServer.Talents;

namespace WCell.RealmServer.Database
{
    //[ActiveRecord("GlyphRecords", Access = PropertyAccess.Property)]
    public class GlyphRecord //: ActiveRecordBase<GlyphRecord>
    {
        [PrimaryKey(PrimaryKeyType.Increment)]
        private long id
        {
            get;
            set;
        }

        public TalentSpecProfile TalentSpecProfile
        {
            get;
            set;
        }

        [Property]
        public int GlyphGroupId
        {
            get;
            set;
        }

        [Field]
        private int glyphPropertiesId;

        public short GlyphPropertiesId
        {
            get { return (short)glyphPropertiesId; }
            set { glyphPropertiesId = value; }
        }

        public GlyphRecord(TalentSpecProfile profile, int groupId)
        {
            TalentSpecProfile = profile;
            GlyphGroupId = groupId;
            glyphPropertiesId = 0;
        }

        private GlyphRecord()
        {
        }

        public static GlyphRecord NewGlyphRecord(TalentSpecProfile profile, int groupId)
        {
            var newRecord = new GlyphRecord() {
                TalentSpecProfile = profile,
                GlyphGroupId = groupId,
                glyphPropertiesId = 0
            };

            //newRecord.CreateAndFlush();

            return newRecord;
        }

        // Glyph Info goes here:
    }
}