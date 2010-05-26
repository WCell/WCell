/*************************************************************************
 *
 *   file		: SpellCommands.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-29 04:07:03 +0100 (fr, 29 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1232 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
	public class ClearCooldownsCommand : RealmServerCommand
	{
		protected ClearCooldownsCommand() { }

		protected override void Initialize()
		{
			Init("ClearCooldowns");
			EnglishDescription = "Clears all reamining spell-cooldowns";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			if (trigger.Args.Target.HasSpells)
			{
				trigger.Args.Target.Spells.ClearCooldowns();
				trigger.Reply("All Cooldowns cleared.");
			}
			else
			{
				trigger.Reply("Target has no Cooldowns.");
			}
		}

		public override ObjectTypeCustom TargetTypes
		{
			get { return ObjectTypeCustom.All; }
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class AddSpellCommand : RealmServerCommand
	{
		protected AddSpellCommand() { }

		protected override void Initialize()
		{
			Init("spelladd", "addspell");
			ParamInfo = "";
			EnglishDescription = "Deprecated - Use \"spell add\" instead.";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			SpellCommand.AddSpellCommand.Instance.Process(trigger);
		}

		public override ObjectTypeCustom TargetTypes
		{
			get { return ObjectTypeCustom.Unit; }
		}
	}

	public class SpellCommand : RealmServerCommand
	{
		protected SpellCommand() { }

		protected override void Initialize()
		{
			Init("Spell", "Spells", "Sp");
			ParamInfo = "";
			EnglishDescription = "Provides commands to interact with the Spells of the Target.";
		}

		#region Add
		public class AddSpellCommand : SubCommand
		{
			private static AddSpellCommand m_instance;
			public static AddSpellCommand Instance
			{
				get { return m_instance; }
			}

			protected AddSpellCommand()
			{
				m_instance = this;
			}

			protected override void Initialize()
			{
				Init("Add", "A");
				ParamInfo = "[-[r][c [<class>]]] [<spell> [<cooldown for AI>]]";
				EnglishDescription = "Adds the given spell. " +
									 "-r (Reagents) switch also adds all constraints required by the Spell (Tools, Reagents, Objects, Skills). " +
									 "-c [<class>] adds all spells of the Character's or a given class." +
									 "-t (talents) adds the highest rank of all spells of all talents";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var mod = trigger.Text.NextModifiers();
				var target = trigger.Args.Target;

				if (mod.Contains("c"))
				{
					ClassId clss;
					if (trigger.Text.HasNext)
					{
						clss = trigger.Text.NextEnum(ClassId.End);
						if (clss == ClassId.End)
						{
							trigger.Reply("Invalid Class.");
							return;
						}
					}
					else
					{
						clss = target.Class;
					}

					var count = target.Spells.Count;
					var lines = SpellLines.GetLines(clss);
					foreach (var line in lines)
					{
						if (mod.Contains("r"))
						{
							((Character)target).PlayerSpells.SatisfyConstraintsFor(line.HighestRank);
						}
						if (line.HighestRank.Talent == null)
						{
							target.Spells.AddSpell(line.HighestRank);
						}
					}
					if (count > 0)
					{
						trigger.Reply("Added {0} spells.", count);
					}
				}
				else
				{
					var id = trigger.Text.NextEnum(SpellId.None);
					var spell = SpellHandler.Get(id);

					if (spell != null)
					{
						target.EnsureSpells().AddSpell(spell);
						if (mod.Contains("r") && target is Character)
						{
							((Character)target).PlayerSpells.SatisfyConstraintsFor(spell);
						}
						trigger.Reply("Spell added: " + spell);
					}
					else
					{
						trigger.Reply("Spell doesn't exist.");
					}
				}

				if (mod.Contains("t"))
				{
					int count = 0;
					var lines = SpellLines.GetLines(target.Class);
					foreach (var line in lines)
					{
						if (mod.Contains("r"))
						{
							((Character)target).PlayerSpells.SatisfyConstraintsFor(line.HighestRank);
						}
						if (line.HighestRank.Talent != null)
						{
							target.Spells.AddSpell(line.HighestRank);
							count++;
						}
					}
					trigger.Reply("Added {0} talents", count);
				}
			}
		}
		#endregion

		#region Remove
		public class RemoveSpellCommand : SubCommand
		{
			protected RemoveSpellCommand() { }

			protected override void Initialize()
			{
				Init("Remove", "R");
				ParamInfo = "<spell>";
				EnglishDescription = "Removes the given Spell";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var id = trigger.Text.NextEnum(SpellId.None);
				var spell = SpellHandler.Get(id);

				if (spell != null)
				{
					if (trigger.Args.Target.HasSpells)
					{
						trigger.Args.Target.Spells.Remove(spell);
						trigger.Reply("Spell removed: " + spell);
					}
				}
				else
				{
					trigger.Reply("Spell {0} doesn't exist.", id);
				}
			}
		}
		#endregion

		#region Purge
		public class PurgeSpellsCommand : SubCommand
		{
			protected PurgeSpellsCommand() { }

			protected override void Initialize()
			{
				Init("Purge");
				EnglishDescription = "Removes all spells but the initial ones.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				if (trigger.Args.Target.HasSpells)
				{
					trigger.Args.Target.Spells.Clear();
					trigger.Args.Target.Spells.AddDefaultSpells();
					trigger.Reply("Purged spells.");
				}
				else
				{
					trigger.Reply("Target has no Spells.");
				}
			}
		}
		#endregion

		#region Trigger
		public class TriggerpellCommand : SubCommand
		{
			protected TriggerpellCommand()
			{
			}

			protected override void Initialize()
			{
				Init("Trigger", "T");
				ParamInfo = "<spellid>";
				EnglishDescription = "Triggers the given spell on the target of the command.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var target = trigger.Args.Target;
				var id = trigger.Text.NextEnum(SpellId.None);
				var spell = SpellHandler.Get(id);

				if (spell != null)
				{
					target.SpellCast.TriggerSelf(spell);
					trigger.Reply("Spell triggered: " + spell);
				}
				else
				{
					trigger.Reply("Spell doesn't exist.");
				}
			}
		}
		#endregion

		public override ObjectTypeCustom TargetTypes
		{
			get
			{
				return ObjectTypeCustom.Unit;
			}
		}
	}

	#region Spell Visual
	public class SpellVisualCommand : RealmServerCommand
	{
		protected SpellVisualCommand() { }

		protected override void Initialize()
		{
			Init("SpellVisual", "PlaySpellVisual", "SpellAnim");
			ParamInfo = "<SpellId>";
			EnglishDescription = "Plays the visual of the given spell";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var id = trigger.Text.NextEnum(SpellId.None);
			var spell = SpellHandler.Get(id);
			if (spell == null)
			{
				trigger.Reply("Invalid SpellId: " + id);
			}
			else
			{
				var visual = spell.Visual;
				SpellHandler.SendVisual(trigger.Args.Target, visual);
			}
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
