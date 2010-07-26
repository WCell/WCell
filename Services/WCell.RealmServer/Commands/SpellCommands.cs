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

using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Constants.Talents;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Talents;
using WCell.Util;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
	public class ClearCooldownsCommand : RealmServerCommand
	{
		protected ClearCooldownsCommand() { }

		protected override void Initialize()
		{
			Init("ClearCooldowns");
			Description = new TranslatableItem(LangKey.CmdSpellClearDescription);
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			if (trigger.Args.Target.HasSpells)
			{
				trigger.Args.Target.Spells.ClearCooldowns();
				trigger.Reply(LangKey.CmdSpellClearResponse);
			}
			else
			{
				trigger.Reply(LangKey.CmdSpellClearError);
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
			EnglishParamInfo = "";
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

	#region GetSpell
	public class SpellGetCommand : RealmServerCommand
	{
		public static Spell[] RetrieveSpells(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var ids = trigger.Text.Remainder.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			var spells = new List<Spell>(ids.Length);
			foreach (var id in ids)
			{
				// try SpellId
				SpellId sid;
				Spell spell = null;
				if (EnumUtil.TryParse(id, out sid))
				{
					spell = SpellHandler.Get(sid);
				}

				if (spell == null)
				{
					// try SpellLine name
					SpellLineId lineid;
					if (EnumUtil.TryParse(id, out lineid))
					{
						var line = SpellLines.GetLine(lineid);
						if (line != null)
						{
							spell = line.HighestRank;
						}
					}

					if (spell == null)
					{
						// try talent name
						var talentId = trigger.Text.NextEnum(TalentId.None);
						var talent = TalentMgr.GetEntry(talentId);
						if (talent != null && talent.Spells != null && talent.Spells.Length > 0)
						{
							spell = talent.Spells[talent.Spells.Length - 1]; // add highest rank
						}

						if (spell == null)
						{
							continue;
						}
					}
				}
				spells.Add(spell);
			}
			return spells.ToArray();
		}

		protected override void Initialize()
		{
			Init("GetSpell", "SpellGet");
			Description = new TranslatableItem(LangKey.CmdSpellGetDescription);
			ParamInfo = new TranslatableItem(LangKey.CmdSpellGetParamInfo);
		}

		public override object Eval(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var spells = RetrieveSpells(trigger);
			if (spells.Length == 0)
			{
				return null;
			}
			return spells.Length > 1 ? (object)spells : spells[0];
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var spell = RetrieveSpells(trigger);
			trigger.Reply(spell.ToString());
		}

		public override ObjectTypeCustom TargetTypes
		{
			get { return ObjectTypeCustom.None; }
		}
	}
	#endregion

	public class SpellCommand : RealmServerCommand
	{
		protected SpellCommand() { }

		protected override void Initialize()
		{
			Init("Spell", "Spells", "Sp");
			EnglishParamInfo = "";
			Description = new TranslatableItem(LangKey.CmdSpellDescription);
		}

		#region Add
		public class AddSpellCommand : SubCommand
		{
			public static AddSpellCommand Instance { get; private set; }

			protected AddSpellCommand()
			{
				Instance = this;
			}

			protected override void Initialize()
			{
				Init("Add", "A");
				ParamInfo = new TranslatableItem(LangKey.CmdSpellAddParamInfo);
				Description = new TranslatableItem(LangKey.CmdSpellAddDescription);
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var mod = trigger.Text.NextModifiers();
				var target = trigger.Args.Target;

				if (mod.Length > 0)
				{
					if (mod.Contains("c"))
					{
						// add all class abilities
						ClassId clss;
						if (trigger.Text.HasNext)
						{
							clss = trigger.Text.NextEnum(ClassId.End);
							if (clss == ClassId.End)
							{
								trigger.Reply(LangKey.InvalidClass);
								return;
							}
						}
						else
						{
							clss = target.Class;
						}

						var count = 0;
						var lines = SpellLines.GetLines(clss);
						foreach (var line in lines)
						{
							if (line.HighestRank.Talent == null)
							{
								AddSpell(target, line.HighestRank, mod.Contains("r"));
								count++;
							}
						}
						if (count > 0)
						{
							trigger.Reply(LangKey.CmdSpellAddResponseSpells, count);
						}
					}

					if (mod.Contains("t"))
					{
						// add all talents
						int count = 0;
						var lines = SpellLines.GetLines(target.Class);
						foreach (var line in lines)
						{
							if (line.HighestRank.Talent != null)
							{
								AddSpell(target, line.HighestRank, mod.Contains("r"));
								count++;
							}
						}
						trigger.Reply(LangKey.CmdSpellAddResponseTalents, count);
					}
				}
				else
				{
					// add list of spells
					var spells = SpellGetCommand.RetrieveSpells(trigger);
					if (spells.Length == 0)
					{
						trigger.Reply(LangKey.CmdSpellNotExists);
					}
					else
					{
						foreach (var spell in spells)
						{
							AddSpell(target, spell, mod.Contains("r"));
							trigger.Reply(LangKey.CmdSpellAddResponseSpell + spell.ToString());
						}
					}
				}
			}

			private static void AddSpell(Unit target, Spell spell, bool addRequired)
			{
				var chr = target as Character;
				if (addRequired && chr != null)
				{
					chr.PlayerSpells.SatisfyConstraintsFor(spell);
				}
				else
				{
					// Profession
					if (spell.Skill != null && chr != null)
					{
						chr.Skills.TryLearn(spell.SkillId);
					}
				}

				if (spell.Talent != null && chr != null)
				{
					// talent
					chr.Talents.Set(spell.Talent, spell.Line.SpellCount - 1);
				}
				else
				{
					// normal spell
					target.EnsureSpells().AddSpell(spell);
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
				ParamInfo = new TranslatableItem(LangKey.CmdSpellRemoveParamInfo);
				Description = new TranslatableItem(LangKey.CmdSpellRemoveDescription);
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
						trigger.Reply(LangKey.CmdSpellRemoveResponse + spell.ToString());
					}
				}
				else
				{
					trigger.Reply(LangKey.CmdSpellRemoveError, id);
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
				Description = new TranslatableItem(LangKey.CmdSpellPurgeDescription);
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				if (trigger.Args.Target.HasSpells)
				{
					trigger.Args.Target.Spells.Clear();
					trigger.Args.Target.Spells.AddDefaultSpells();
					trigger.Reply(LangKey.CmdSpellPurgeResponse);
				}
				else
				{
					trigger.Reply(LangKey.CmdSpellPurgeError);
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
				ParamInfo = new TranslatableItem(LangKey.CmdSpellTriggerParamInfo);
				Description = new TranslatableItem(LangKey.CmdSpellTriggerDescription);
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var target = trigger.Args.Target;
				var id = trigger.Text.NextEnum(SpellId.None);
				var spell = SpellHandler.Get(id);

				if (spell != null)
				{
					target.SpellCast.TriggerSelf(spell);
					trigger.Reply(LangKey.CmdSpellTriggerResponse + spell.ToString());
				}
				else
				{
					trigger.Reply(LangKey.CmdSpellTriggerError);
				}
			}
		}
		#endregion

		public override ObjectTypeCustom TargetTypes
		{
			get { return ObjectTypeCustom.Unit; }
		}
	}

	#region Spell Visual
	public class SpellVisualCommand : RealmServerCommand
	{
		protected SpellVisualCommand() { }

		protected override void Initialize()
		{
			Init("SpellVisual", "PlaySpellVisual", "SpellAnim");
			ParamInfo = new TranslatableItem(LangKey.CmdSpellVisualParamInfo);
			Description = new TranslatableItem(LangKey.CmdSpellVisualDescription);
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var id = trigger.Text.NextEnum(SpellId.None);
			var spell = SpellHandler.Get(id);
			if (spell == null)
			{
				trigger.Reply(LangKey.CmdSpellVisualError, id);
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