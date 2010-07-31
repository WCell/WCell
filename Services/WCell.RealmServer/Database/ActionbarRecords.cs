using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.ActiveRecord;
using WCell.RealmServer.Talents;

namespace WCell.RealmServer.Database
{
    //[ActiveRecord("ActionbarRecords", Access = PropertyAccess.Property)]
    public class ActionbarRecord //: ActiveRecordBase<ActionbarRecord>
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
        public int ActionbarGroupId
        {
            get;
            set;
        }

        private ActionbarRecord()
        {
        }

        public static ActionbarRecord NewActionBarRecord(SpecProfile profile, int groupId)
        {
            var newRecord = new ActionbarRecord() {
                SpecProfile = profile,
                ActionbarGroupId = groupId
            };

            //newRecord.CreateAndFlush();

            return newRecord;
        }

        // Actionbar Info goes here:
    }
}