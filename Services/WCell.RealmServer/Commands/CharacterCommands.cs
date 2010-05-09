using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Formulas;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
	#region GiveXP
	public class GiveXPCommand : RealmServerCommand
	{
		protected GiveXPCommand() { }

		protected override void Initialize()
		{
			base.Init("GiveXP", "XP", "Exp");
			ParamInfo = "<amount>";
			EnglishDescription = "Gives the given amount of experience.";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var chr = ((Character)trigger.Args.Target);
			var xp = trigger.Text.NextInt(1);

			chr.GainXp(xp, false);
		}

		public override ObjectTypeCustom TargetTypes
		{
			get
			{
				return ObjectTypeCustom.Player;
			}
		}
	}
	#endregion

	#region Level
	public class LevelCommand : RealmServerCommand
	{
		protected LevelCommand() { }

		protected override void Initialize()
		{
			Init("Level");
			ParamInfo = "<level>";
			EnglishDescription = "Sets the player to the given level. Level must be higher than the player's level.";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var chr = ((Character)trigger.Args.Target);
			var level = trigger.Text.NextInt(1);

			if (level <= chr.Level)
			{
				trigger.Reply("You cannot remove levels!");
				return;
			}

			int xpToLevel = 0;

			for (var i = chr.Level; i < level && i < RealmServerConfiguration.MaxCharacterLevel; i++)
			{
				xpToLevel += XpGenerator.XpTable[i];
			}

			// remove their current xp so they get enough xp to be 0xp into their final level
			xpToLevel -= chr.XP;

			chr.GainXp(xpToLevel, false);
		}

		public override ObjectTypeCustom TargetTypes
		{
			get
			{
				return ObjectTypeCustom.Player;
			}
		}
	}
	#endregion

	#region Bank
	public class BankCommand : RealmServerCommand
	{
		protected BankCommand() { }

		protected override void Initialize()
		{
			base.Init("Bank");
			ParamInfo = "";
			EnglishDescription = "Opens the bank for the target through oneself (if one leaves the target, it won't be allowed to continue using the Bank).";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			((Character)trigger.Args.Target).OpenBank(trigger.Args.Character);
		}

		public override ObjectTypeCustom TargetTypes
		{
			get
			{
				return ObjectTypeCustom.Player;
			}
		}
	}
	#endregion

	#region Exploration
	public class ExploreCommand : RealmServerCommand
	{
		protected ExploreCommand() { }

		protected override void Initialize()
		{
			Init("Explore");
			ParamInfo = "[<zone>]";
			EnglishDescription = "Explores the map. If zone is given it will toggle exploration of that zone, else it will explore all zones.";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var chr = (Character)trigger.Args.Target;
			var zone = trigger.Text.NextEnum(ZoneId.None);
			if (zone == ZoneId.None)
			{
				for (var i = PlayerFields.EXPLORED_ZONES_1;
					i < (PlayerFields)((uint)PlayerFields.EXPLORED_ZONES_1 + UpdateFieldMgr.ExplorationZoneFieldSize); i++)
				{
					chr.SetUInt32(i, uint.MaxValue);
				}
			}
			else
			{
				chr.SetZoneExplored(zone, !chr.IsZoneExplored(zone));
			}
		}

		public override ObjectTypeCustom TargetTypes
		{
			get
			{
				return ObjectTypeCustom.Player;
			}
		}
	}
	#endregion
}
