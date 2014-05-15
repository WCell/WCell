using System;
using System.Linq;
using System.Threading;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.Misc
{
	//[ActiveRecord(Access = PropertyAccess.Property)] TOOD: Map this
    public class BugReport //: WCellRecord<BugReport>
    {
		#region Static Members

		//private static readonly NHIdGenerator _idGenerator = new NHIdGenerator(typeof(BugReport), "_id");

		private static bool _idGeneratorInitialised;
		private static long _highestId;

		private static void Init()
		{
			//long highestId;
			try
			{
				_highestId = RealmWorldDBMgr.DatabaseProvider.Query<BugReport>().Max(bugReport => bugReport.Id);
			}
			catch (Exception e)
			{
				RealmWorldDBMgr.OnDBError(e);
				_highestId = RealmWorldDBMgr.DatabaseProvider.Query<BugReport>().Max(bugReport => bugReport.Id);
			}

			//_highestId = (long)Convert.ChangeType(highestId, typeof(long));

			_idGeneratorInitialised = true;
		}

		/// <summary>
		/// Returns the next unique Id for a new Item
		/// </summary>
		public static long NextId()
		{
			if (!_idGeneratorInitialised)
				Init();

			return Interlocked.Increment(ref _highestId);
		}

		public static long LastId
		{
			get
			{
				if (!_idGeneratorInitialised)
					Init();
				return Interlocked.Read(ref _highestId);
			}
		}



		public static BugReport CreateNewBugReport(string type, string content)
        {
            BugReport report;
            report = new BugReport
            {
                _id = (int)NextId(),
                _type = type,
                _content = content,
                _reportDate = DateTime.Now,
                //State = RecordState.New
            };
			RealmWorldDBMgr.DatabaseProvider.Save(report);
            return report;
        }
		#endregion

		private int _id;

		//[PrimaryKey(PrimaryKeyType.Assigned, "Id")]
		public int Id
		{
			get;
			set;
		}

        //[Field("Type", NotNull = true)]
        private string _type;

        //[Field("Content", NotNull = true)]
        private string _content;

        //[Field("Created", NotNull = true)]
        private DateTime _reportDate;

        public override string ToString()
        {
            return string.Format("{0} - ID : {1}, Type : {2}, Content : {3}, Created : {4}", GetType(), _id, _type, _content, _reportDate);
        }
    }
}
