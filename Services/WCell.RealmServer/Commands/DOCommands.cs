using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.Util.Commands;
using WCell.Constants.Updates;

namespace WCell.RealmServer.Commands
{
	// This file contains commands for creating and destruction DynamicObjects


	public class SpawnDOCommand : RealmServerCommand
	{
		protected SpawnDOCommand() { }

		protected override void Initialize()
		{
			base.Init("SpawnDO");
			ParamInfo = "<spellid> <radius> [<scale>]";
			EnglishDescription = "Spawns a new DynamicObjects with the given parameters";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var spellId = trigger.Text.NextEnum(SpellId.None);
			var radius = trigger.Text.NextFloat(5);
			var scale = trigger.Text.NextFloat(1);
			var dynObj = new DynamicObject(trigger.Args.Character, spellId, radius,
				trigger.Args.Target.Region, trigger.Args.Target.Position)
				{
					ScaleX = scale
				};

			SpellHandler.StaticDOs.Add(dynObj.EntityId, dynObj);
			trigger.Reply("DynamicObject created.");
		}

		public override ObjectTypeCustom TargetTypes
		{
			get { return ObjectTypeCustom.All; }
		}
	}

	public class ClearDOsCommand : RealmServerCommand
	{
		protected ClearDOsCommand() { }

		protected override void Initialize()
		{
			base.Init("ClearDOs");
			EnglishDescription = "Removes all staticly spawned DOs";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			foreach (var dynObj in SpellHandler.StaticDOs.Values)
			{
				dynObj.Delete();
			}
			SpellHandler.StaticDOs.Clear();
		}

	}

	public class ListDOs : RealmServerCommand
	{
		protected override void Initialize()
		{
			base.Init("ListDOs");
			EnglishDescription = "Shows a list of all available Dynamic Objects";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			foreach (var doSpell in SpellHandler.DOSpells.Values)
			{
				trigger.Reply("{0} (Id: {1})", doSpell.Name, doSpell.Id);
			}
		}
	}
}
