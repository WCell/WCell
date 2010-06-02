using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Intercommunication.DataTypes;
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


	#region Debug
	public class DebugCommand : AuthServerCommand
	{
		protected override void Initialize()
		{
			Init("Debug");
			EnglishDescription = "Provides Debug-capabilities and management of Debug-tools for Devs.";
		}

		#region GC
		public class GCCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("GC");
				EnglishDescription = "Don't use this unless you are well aware of the stages and heuristics involved in the GC process!";
			}

			public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
			{
				GC.Collect();
				trigger.Reply("Done.");
			}
		}
		#endregion
	}
	#endregion
}
