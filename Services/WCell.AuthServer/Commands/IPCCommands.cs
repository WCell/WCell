using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Commands;
using WCell.AuthServer.IPC;

namespace WCell.AuthServer.Commands
{

	public class AuthIPCCommand : AuthServerCommand
	{
		protected override void Initialize()
		{
			Init("IPC");
			EnglishParamInfo = "";
			EnglishDescription = "Defines a set of Commands to administrate the IPC Service.";
		}

		public class StartCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Start", "Run");
				EnglishParamInfo = "";
				EnglishDescription = "Starts the IPC Service (if not already running).";
			}

			public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
			{
				if (IPCServiceHost.IsOpen)
				{
					trigger.Reply("IPC Service is already running - You need to close it before being able to re-open it.");
				}
				else
				{
					IPCServiceHost.StartService();
					trigger.Reply("Done.");
				}
			}
		}

		public class StopCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Stop", "Halt");
				EnglishParamInfo = "";
				EnglishDescription = "Stops the IPC Service (if running).";
			}

			public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
			{
				if (!IPCServiceHost.IsOpen)
				{
					trigger.Reply("IPC Service is already closed.");
				}
				else
				{
					IPCServiceHost.StopService();
					trigger.Reply("Done.");
				}
			}
		}

		public class ToggleCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Toggle");
				EnglishParamInfo = "[0/1]";
				EnglishDescription = "Toggles the IPC Service.";
			}

			public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
			{
				var open = trigger.Text.NextBool() | !IPCServiceHost.IsOpen;
				if (open)
				{
					if (IPCServiceHost.IsOpen)
					{
						trigger.Reply("IPC Service already running - You need to close it before being able to re-open it.");
					}
					else
					{
						IPCServiceHost.StartService();
						trigger.Reply("Done.");
					}
				}
				else
				{
					if (!IPCServiceHost.IsOpen)
					{
						trigger.Reply("IPC Service is already closed.");
					}
					else
					{
						IPCServiceHost.StopService();
						trigger.Reply("Done.");
					}
				}
			}
		}
	}
}