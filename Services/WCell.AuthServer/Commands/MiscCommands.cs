using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.AuthServer.Stats;
using WCell.Util.Commands;

namespace WCell.AuthServer.Commands
{
	#region Stats
	public class ServerStatsCommand : AuthServerCommand
	{
		protected override void Initialize()
		{
			Init("Stats");
			EnglishDescription = "Provides commands to show and manage server-statistics.";
		}

		public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
		{
			foreach (var line in AuthStats.Instance.GetFullStats())
			{
				trigger.Reply(line);
			}
		}

		//public class DisplayStatsCommand : SubCommand
		//{
		//    protected override void Initialize()
		//    {
		//        Init("Display", "Show", "D");
		//        Description = "Displays the current Server Stats";
		//    }

		//    public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		//    {
		//        foreach (var line in RealmStats.Instance.GetFullStats())
		//        {
		//            trigger.Reply(line);
		//        }
		//    }
		//}
	}
	#endregion
}