using WCell.Constants.Updates;
using WCell.RealmServer.Commands;
using WCell.Util.Commands;
using WCell.RealmServer.Entities;

namespace WCell.Addons.Default.Commands
{
	/// <summary>
	/// A simple command to add/remove the Staff's teleport spell.
	/// All Command-classes are automatically added when the Addon is loaded.
	/// </summary>
	public class TeleSpellCommand : RealmServerCommand
	{
		protected override void Initialize()
		{
			Init("TeleSpell", "ToggleTeleSpell");
			EnglishDescription = "Adds or Removes the custom Teleport spell.";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var chr = ((Character)trigger.Args.Target);
			if (chr.Spells.Contains(DefaultAddon.TeleSpellId))
			{
				chr.Spells.Remove(DefaultAddon.TeleSpellId);
			}
			else
			{
				chr.Spells.AddSpell(DefaultAddon.TeleSpellId);
			}
		}

		public override ObjectTypeCustom TargetTypes
		{
			get { return ObjectTypeCustom.Player; }
		}
	}
}