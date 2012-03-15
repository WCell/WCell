using Castle.ActiveRecord;
using NHibernate.Criterion;

namespace WCell.RealmServer.Database
{
    [ActiveRecord(Table = "AccountData")]
    public class AccountDataRecord : ActiveRecordBase<AccountDataRecord>
    {
        [PrimaryKey(PrimaryKeyType.Assigned)]
        public long accountId { get; set; }

        [Field]
        public byte[][] DataHolder = new byte[8][];

        /// <summary>
        /// TODO: Can be changed to uint (unix time)
        /// </summary>
        [Field]
        public int[] TimeStamps = new int[8];

        [Field]
        public int[] SizeHolder = new int[8];

        public static AccountDataRecord GetAccountData(long accountID)
        {
            return FindOne(Restrictions.Eq("accountId", accountID));
        }

        /// <summary>
        /// This is used to initialize *skeleton* data for accounts that do not already have data stored server side.
        /// We initialize with DateTime.MinValue to cause the client to update the server side data.
        /// </summary>
        /// <param name="accountID">GUID of the account that needs to be initialized</param>
        /// <returns>An AccountDataRecord reference</returns>
        public static AccountDataRecord InitializeNewAccount(long accountID)
        {
            var newData = new AccountDataRecord { accountId = accountID };

            for (uint i = 7; i > 0; i--)
            {
                newData.TimeStamps[i] = 0;
            }

            newData.Create();

            return newData;
        }

        public void SetAccountData(uint dataType, uint time, byte[] data, uint compressedSize)
        {
            DataHolder[dataType] = data;
            //TimeHolder[dataType] = DateTime.Now;
            TimeStamps[dataType] = (int)time;
            SizeHolder[dataType] = (int)compressedSize;
        }
    }
}