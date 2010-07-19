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

        public SpecProfile SpecProfile
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

        public GlyphRecord(SpecProfile profile, int groupId)
        {
            SpecProfile = profile;
            GlyphGroupId = groupId;
            glyphPropertiesId = 0;
        }

        private GlyphRecord()
        {
        }

        public static GlyphRecord NewGlyphRecord(SpecProfile profile, int groupId)
        {
            var newRecord = new GlyphRecord() {
                SpecProfile = profile,
                GlyphGroupId = groupId,
                glyphPropertiesId = 0
            };

            //newRecord.CreateAndFlush();

            return newRecord;
        }

        // Glyph Info goes here:
    }
}