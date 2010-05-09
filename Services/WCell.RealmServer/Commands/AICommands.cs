using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Updates;
using WCell.Util.Commands;
using WCell.RealmServer.Entities;
using WCell.RealmServer.AI.Brains;

namespace WCell.RealmServer.Commands
{


	public class AICommand : RealmServerCommand
	{
		protected AICommand() { }

		protected override void Initialize()
		{
			Init("AI");
			EnglishDescription = "Provides Commands to interact with AI.";
		}

		public class AIActiveCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Active");
				ParamInfo = "<1/0>";
				EnglishDescription = "Activates/Deactivates AI of target.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				bool newState;

				var target = trigger.Args.Target;
				if (target == trigger.Args.Character)
				{
					target = trigger.Args.Character.Target;
				}
				if (!(target is NPC))
				{
					trigger.Reply("Must target NPC.");
					return;
				}

				var brain = target.Brain;
				if (brain == null)
				{
					trigger.Reply(target.Name + " doesn't have a brain.");
					return;
				}

				if (trigger.Text.HasNext)
				{
					newState = trigger.Text.NextBool();
				}
				else
				{
					newState = !brain.IsRunning;
				}

				brain.IsRunning = newState;
				trigger.Reply(target.Name + "'s Brain is now: " + (newState ? "Activated" : "Deactivated"));
			}
		}

		// TODO: Make this work on players too!
		public class AIMoveToMeCommand : SubCommand
		{
			protected AIMoveToMeCommand() { }

			protected override void Initialize()
			{
				Init("MoveToMe", "Come");
				EnglishDescription = "Moves a target NPC to the character.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var target = trigger.Args.Target;
				if (target == trigger.Args.Character)
				{
					target = trigger.Args.Character.Target;
				}
				if (!(target is NPC))
				{
					trigger.Reply("Can only command NPCs.");
				}
				else
				{
					target.MoveToThenIdle(trigger.Args.Character);
				}
			}
		}

		// TODO: Make this work on players too!
		public class AIFollowCommand : SubCommand
		{
			protected AIFollowCommand() { }

			protected override void Initialize()
			{
				Init("Follow");
				EnglishDescription = "Moves a target NPC to the character.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var target = trigger.Args.Target;
				if (target == trigger.Args.Character)
				{
					target = trigger.Args.Character.Target;
				}
				if (!(target is NPC))
				{
					trigger.Reply("Can only command NPCs.");
				}
				else
				{
					target.Follow(trigger.Args.Character);
				}
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
}
