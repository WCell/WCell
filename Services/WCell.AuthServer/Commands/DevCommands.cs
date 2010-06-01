using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Commands;

namespace WCell.AuthServer.Commands
{
	public class DevCommand : AuthServerCommand
	{
		protected override void Initialize()
		{
			Init("Dev");
		}

		public class NetworkToggleCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Network");
			}

			public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
			{
				AuthenticationServer.Instance.TCPEnabled = !AuthenticationServer.Instance.TCPEnabled;
			}
		}
	}
}
