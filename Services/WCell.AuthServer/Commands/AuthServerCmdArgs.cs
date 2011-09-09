using WCell.AuthServer.Accounts;
using WCell.Util.Commands;

namespace WCell.AuthServer.Commands
{
	public class AuthServerCmdArgs : ICmdArgs
	{
		public AuthServerCmdArgs(Account acc)
		{
			Account = acc;
		}

		public Account Account;

		public string Extra;
	}
}