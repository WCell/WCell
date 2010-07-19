using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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