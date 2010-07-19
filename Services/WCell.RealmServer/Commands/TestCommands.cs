using WCell.Constants.Spells;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.NPCs;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
	#region CastFail
	public class ShowCastFailCommand : RealmServerCommand
	{
		protected ShowCastFailCommand() {}

		protected override void Initialize()
		{
			base.Init("ShowCastFail");
			EnglishParamInfo = "<spell> <reason>";
			EnglishDescription = "Sends a spell failed packet";

			// doesn't do anything
			Enabled = false;
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var spell = trigger.Text.NextEnum(SpellId.None);
			var reason = trigger.Text.NextEnum(SpellFailedReason.Interrupted);
			//SpellHandler.SendCastFailed(trigger.Args.Target, trigger.Args.Character.Client, spell, reason);
			trigger.Reply("Done.");
		}
	}
	#endregion

	#region BankSlotResult
	public class ShowBankSlotResultCommand : RealmServerCommand
	{
		protected ShowBankSlotResultCommand() {}

		protected override void Initialize()
		{
			base.Init("ShowBankSlotResult");
			EnglishParamInfo = "<value>";
			EnglishDescription = "Sends the BankSlotResult packet";

			// we don't really need this
			Enabled = false;
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var value = trigger.Text.NextEnum(BuyBankBagResponse.Ok);
			NPCHandler.SendBankSlotResult(trigger.Args.Character, value);
			trigger.Reply("Done.");
		}

		public override bool NeedsCharacter
		{
			get
			{
				return true;
			}
		}
	}
	#endregion
}