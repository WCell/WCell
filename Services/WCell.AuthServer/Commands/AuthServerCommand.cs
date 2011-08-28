using WCell.Util.Commands;

namespace WCell.AuthServer.Commands
{
	public abstract class AuthServerCommand : Command<AuthServerCmdArgs>
	{
		public virtual bool RequiresAccount
		{
			get
			{
				return false;
			}
		}
	}
}