using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.ActiveRecord;
using NHibernate.Criterion;
using NLog;
using WCell.Constants.Achievements;
using WCell.Core.Database;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Misc
{
    [ActiveRecord(Access = PropertyAccess.Property)]
    public class BugReport : WCellRecord<BugReport>
    {
        #region Static Members

        private static readonly NHIdGenerator _idGenerator = new NHIdGenerator(typeof(BugReport), "_id");

        public static BugReport CreateNewBugReport(string type, string content)
        {
            BugReport report;
            report = new BugReport
            {
                _id = (int)_idGenerator.Next(),
                _type = type,
                _content = content,
                _reportDate = DateTime.Now,
                New = true,
            };

            return report;
        }
        #endregion
        
        [PrimaryKey(PrimaryKeyType.Assigned, "Id")]
        private int _id
        {
            get;
            set;
        }

        [Field("Type", NotNull = true)]
        private string _type;

        [Field("Content", NotNull = true)]
        private string _content;

        [Field("Created", NotNull = true)]
        private DateTime _reportDate;

        public override string ToString()
        {
            return string.Format("{0} - ID : {1}, Type : {2}, Content : {3}, Created : {4}", GetType(), _id, _type, _content, _reportDate);
        }
    }
}
