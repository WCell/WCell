using WCell.Addons.Default.Samples;
using WCell.Constants.Updates;
using WCell.RealmServer.Commands;
using WCell.Intercommunication.DataTypes;
using WCell.Util.Commands;
using WCell.RealmServer.Entities;
using WCell.RealmServer.AI;

namespace WCell.Addons.Default.Commands
{
	#region Samples
	public class SampleCommand : RealmServerCommand
	{
		public override RoleStatus RequiredStatusDefault
		{
			get
			{
				return RoleStatus.Admin;
			}
		}

		protected override void Initialize()
		{
			Init("Sample", "Samp");
			EnglishDescription = "Provides commands for developers to create and handle Sample data for testing.";
		}

		public class SampleNPCCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("NPC");
				EnglishParamInfo = "[-f]";
				EnglishDescription = "Adds a sample NPC. -f makes it friendly.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var entry = MixedSamples.GrizzlyBear;

				var mob = entry.Create();
				var target = trigger.Args.Target;
				var pos = target.Position;

				mob.SetZone(target.Zone);

				mob.Brain.DefaultState = BrainState.Idle;

				var mod = trigger.Text.NextModifiers();
				if (mod == "f")
				{
					mob.Faction = target.Faction;
				}

				trigger.Reply("Created {0}.", entry.DefaultName);

				target.PlaceInFront(mob);
			}
		}

		public class SampleBowCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Bow");
				EnglishParamInfo = "[-ea] [<amount>]";
				EnglishDescription = "Adds a sample Bow." +
					"-a switch auto-equips, -e switch only adds if not already present.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var templ = MixedSamples.Bow;

				var chr = trigger.Args.Target as Character;
				if (chr == null)
				{
					trigger.Reply("Command requires Character as Target.");
				}
				else
				{
					var mods = trigger.Text.NextModifiers();
					var amount = trigger.Text.NextInt(1);

					InventoryCommand.AddItemCommand.AddItem(chr, templ, amount, mods.Contains("a"), mods.Contains("e"));
				}
			}
		}

		public override ObjectTypeCustom TargetTypes
		{
			get { return ObjectTypeCustom.All; }
		}
	}
	#endregion
}