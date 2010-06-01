using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.Util;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
	#region Kill
	public class KillCommand : RealmServerCommand
	{
		protected KillCommand() { }

		protected override void Initialize()
		{
			Init("Kill");
			EnglishDescription = "Kills your current target.";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var target = trigger.Args.Target;
			if (target == trigger.Args.Character)
			{
				target = target.Target;
				if (target == null)
				{
					trigger.Reply("Invalid Target.");
					return;
				}
			}

			SpellHandler.SendVisual(target, SpellId.Lightning);
			if (target is NPC)
			{
				var npc = (NPC)target;
				if (npc.FirstAttacker == null)
				{
					npc.FirstAttacker = trigger.Args.Character;
				}
			}
			target.Kill();
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

	#region Resurrect
	public class ResurrectCommand : RealmServerCommand
	{
		protected ResurrectCommand() { }

		protected override void Initialize()
		{
			Init("Resurrect", "Res");
			EnglishDescription = "Resurrects the Unit";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var target = trigger.Args.Target;
			if (target != null)
			{
				target.Resurrect();
			}
		}

		public override ObjectTypeCustom TargetTypes
		{
			get { return ObjectTypeCustom.Unit; }
		}
	}
	#endregion

	#region Health
	public class HealthCommand : RealmServerCommand
	{
		protected HealthCommand() { }

		protected override void Initialize()
		{
			Init("Health");
			EnglishParamInfo = "<amount>";
			EnglishDescription = "Sets Basehealth to the given value and fills up Health.";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var target = trigger.Args.Target;
			if (target != null)
			{
				var val = trigger.Text.NextInt(1);
				target.BaseHealth = val;
				target.Heal(target.MaxHealth - target.Health);
			}
		}

		public override ObjectTypeCustom TargetTypes
		{
			get { return ObjectTypeCustom.Unit; }
		}
	}
	#endregion

	#region Resurrect
	public class RaceCommand : RealmServerCommand
	{
		protected RaceCommand() { }

		protected override void Initialize()
		{
			Init("Race", "SetRace");
			EnglishParamInfo = "<race>";
			EnglishDescription = "Sets the Unit's race. Also adds the Race's language.";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var word = trigger.Text.NextWord();
			RaceId race;
			if (EnumUtil.TryParse(word, out race))
			{
				trigger.Args.Target.Race = race;
				if (trigger.Args.Target is Character)
				{
					var desc = LanguageHandler.GetLanguageDescByRace(race);
					((Character)trigger.Args.Target).AddLanguage(desc);
				}
			}
			else
			{
				trigger.Reply("Invalid Race: " + word);
			}
		}

		public override ObjectTypeCustom TargetTypes
		{
			get
			{
				return ObjectTypeCustom.Unit;
			}
		}
	}
	#endregion

	#region Invul
	public class InvulModeCommand : RealmServerCommand
	{
		protected InvulModeCommand() { }

		protected override void Initialize()
		{
			Init("Invul");
			EnglishParamInfo = "[0|1]";
			EnglishDescription = "Toggles Invulnerability";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var target = trigger.Args.Target;
			var mode = trigger.Text.NextBool(!target.IsInvulnerable);
			target.IsInvulnerable = mode;
			trigger.Reply("{0} is now " + (mode ? "Invulnerable" : "Vulnerable"), target.Name);
		}

		public override ObjectTypeCustom TargetTypes
		{
			get
			{
				return ObjectTypeCustom.Unit;
			}
		}
	}
	#endregion

	#region Talking
	public class SayCommand : RealmServerCommand
	{
		protected SayCommand() { }

		protected override void Initialize()
		{
			Init("Say");
			EnglishParamInfo = "<text>";
			EnglishDescription = "Say something";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var target = trigger.Args.Target;
			var text = trigger.Text.Remainder.Trim();
			target.Say(text);
		}

		public override ObjectTypeCustom TargetTypes
		{
			get
			{
				return ObjectTypeCustom.All;
			}
		}
	}

	public class YellCommand : RealmServerCommand
	{
		protected YellCommand() { }

		protected override void Initialize()
		{
			Init("Yell");
			EnglishParamInfo = "<text>";
			EnglishDescription = "Yell something";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var target = trigger.Args.Target;
			var text = trigger.Text.Remainder.Trim();
			target.Yell(text);
		}

		public override ObjectTypeCustom TargetTypes
		{
			get
			{
				return ObjectTypeCustom.All;
			}
		}
	}
	#endregion
}
