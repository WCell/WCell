using WCell.Constants.Updates;
using WCell.RealmServer.Content;
using WCell.Util.Commands;
using WCell.Intercommunication.DataTypes;

namespace WCell.RealmServer.Commands
{
	public class ContentCommand : RealmServerCommand
	{
		protected override void Initialize()
		{
			Init("Content", "Cont");
			EnglishDescription = "Provides commands to manage the static content.";
		}

		public override RoleStatus RequiredStatusDefault
		{
			get
			{
				return RoleStatus.Admin;
			}
		}

		public override bool RequiresCharacter
		{
			get
			{
				return false;
			}
		}

		public override ObjectTypeCustom TargetTypes
		{
			get
			{
				return ObjectTypeCustom.None;
			}
		}

		public class ContentLoadCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Load", "L", "Reload");
				EnglishDescription = "Reloads the content-definitions. This is useful when applying changes to the underlying content system.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				trigger.Reply("Loading content-mapping information...");
				ContentMgr.Load();
				trigger.Reply("Done.");
			}
		}

		public class ContentCheckCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Check", "Ch", "C");
				EnglishDescription = "Checks whether all currently loaded content-definitions are correctly reflecting the DB-structure.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				trigger.Reply("Checking Content-Definitions...");
				var count = ContentMgr.Check(trigger.Reply);
				trigger.Reply("Done - Found {0} error(s).", count); 
			}
		}
	}
}