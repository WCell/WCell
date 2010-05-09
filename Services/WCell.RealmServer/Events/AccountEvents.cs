namespace WCell.RealmServer
{
	public partial class RealmAccount
	{
		public delegate void AccountHandler(RealmAccount acc);

		/// <summary>
		/// Is called when the Account logs in
		/// </summary>
		public static event AccountHandler LoggedIn;

		/// <summary>
		/// Is called when the Account logs out
		/// </summary>
		public static event AccountHandler LoggedOut;
	}
}
