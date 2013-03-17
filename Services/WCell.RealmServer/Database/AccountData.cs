using NHibernate.Criterion;
using WCell.Database;

namespace WCell.RealmServer.Database
{
	public class AccountData
	{
		public long AccountId { get; set; }

		public byte[][] DataHolder = new byte[8][];
		/// <summary>
		/// TODO: Can be changed to uint (unix time)
		/// </summary>
		public int[] TimeStamps = new int[8];

		public int[] SizeHolder = new int[8];

		public static AccountData GetAccountData(long accountID)
		{
			var databaseProvider = new DatabaseProvider();
			return databaseProvider.FindOne<AccountData>(Restrictions.Eq("AccountId", accountID));
		}

		/// <summary>
		/// This is used to initialize *skeleton* data for accounts that do not already have data stored server side.
		/// We initialize with DateTime.MinValue to cause the client to update the server side data.
		/// </summary>
		/// <param name="accountID">GUID of the account that needs to be initialized</param>
		/// <returns>An AccountData reference</returns>
		public static AccountData InitializeNewAccount(long accountID)
		{
			var newData = new AccountData {AccountId = accountID};

			for (uint i = 7; i > 0; i--)
			{
				newData.TimeStamps[i] = 0;
			}
			
			return newData;
		}

		public void SetAccountData(uint dataType, uint time, byte[] data, uint compressedSize)
		{
			DataHolder[dataType] = data;
			//TimeHolder[dataType] = DateTime.Now;
			TimeStamps[dataType] = (int) time;
			SizeHolder[dataType] = (int)compressedSize;
		}
	}
}