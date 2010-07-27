/*************************************************************************
 *
 *   file			: SkillCommands.cs
 *   copyright		: (C) The WCell Team
 *   email			: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-24 00:08:42 +0100 (sø, 24 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1212 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using WCell.Constants.Items;
using WCell.Constants.Skills;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Skills;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
	public class SkillCommand : RealmServerCommand
	{
		protected SkillCommand() { }

		protected override void Initialize()
		{
			base.Init("Skill", "Skills", "Sk");
			Description = new TranslatableItem(LangKey.CmdSkillDescription);
		}

		public override ObjectTypeCustom TargetTypes
		{
			get
			{
				return ObjectTypeCustom.Player;
			}
		}


		public class SetCommand : SubCommand
		{
			protected SetCommand() { }

			protected override void Initialize()
			{
				Init("Set", "S");
				ParamInfo = new TranslatableItem(LangKey.CmdSkillSetParamInfo);
				Description = new TranslatableItem(LangKey.CmdSkillSetDescription);
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var id = trigger.Text.NextEnum(SkillId.None);
				var amount = trigger.Text.NextUInt(1);
				var max = Math.Max(trigger.Text.NextUInt(1), amount);

				var skillLine = SkillHandler.Get(id);
				if (skillLine != null)
				{
					var skill = ((Character)trigger.Args.Target).Skills.GetOrCreate(id, true);
					skill.CurrentValue = (ushort)amount;
					skill.MaxValue = (ushort)max;
					trigger.Reply(LangKey.CmdSkillSetResponse, skillLine, amount, max);
				}
				else
				{
					trigger.Reply(LangKey.CmdSkillSetError, id);
				}
			}
		}

		public class LearnCommand : SubCommand
		{
			protected LearnCommand() { }

			protected override void Initialize()
			{
				Init("Learn", "L");
				ParamInfo = new TranslatableItem(LangKey.CmdSkillLearnParamInfo);
				Description = new TranslatableItem(LangKey.CmdSkillLearnDescription);
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var mod = trigger.Text.NextModifiers();
				var id = trigger.Text.NextEnum(SkillId.None);
				var amount = trigger.Text.NextUInt(0);
				var chr = (Character)trigger.Args.Target;
				var inv = chr.Inventory;

				var skillLine = SkillHandler.Get(id);
				if (skillLine != null)
				{
					var skill = ((Character)trigger.Args.Target).Skills.GetOrCreate(id, true);
					skill.CurrentValue = amount > 0 ? (ushort)amount : (ushort)skillLine.MaxValue;
					skill.MaxValue = (ushort)skillLine.MaxValue;
					trigger.Reply(LangKey.CmdSkillLearnResponse, skillLine, amount > 0 ? amount : skillLine.MaxValue);
					if (mod == "r")
					{
						// add bags
						for (var i = InventorySlot.Bag1; i <= InventorySlot.BagLast; i++)
						{
							if (inv[i] == null)
							{
								inv.AddUnchecked((int)i, ItemId.FororsCrateOfEndlessResistGearStorage, 1, true);
							}
						}
					}

					var count = 0;
					foreach (var ability in SkillHandler.GetAbilities(id))
					{
						if (ability.GreyValue < 1)
						{
							continue;
						}
						count++;
						if (count > amount)
						{
							break;
						}
						chr.Spells.AddSpell(ability.Spell);
						if (mod == "r")
						{
							chr.PlayerSpells.SatisfyConstraintsFor(ability.Spell);
						}
					}
				}
				else
				{
					trigger.Reply(LangKey.CmdSkillLearnError, id);
				}
			}
		}


		public class TierCommand : SubCommand
		{
			protected TierCommand() { }

			protected override void Initialize()
			{
				Init("Tier", "SetTier", "ST");
				ParamInfo = new TranslatableItem(LangKey.CmdSkillTierParamInfo);
				Description = new TranslatableItem(LangKey.CmdSkillTierDescription);;
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var id = trigger.Text.NextEnum(SkillId.None);
				if (trigger.Text.HasNext)
				{
					var tier = trigger.Text.NextEnum(SkillTierId.GrandMaster);

					var skillLine = SkillHandler.Get(id);
					if (skillLine != null)
					{
						var skill = ((Character)trigger.Args.Target).Skills.GetOrCreate(id, tier, true);
						trigger.Reply(LangKey.CmdSkillTierResponse, skill, skill.CurrentValue, skill.MaxValue);
					}
					else
					{
						trigger.Reply(LangKey.CmdSkillTierError1, id);
					}
				}
				else
				{
					trigger.Reply(LangKey.CmdSkillTierError2);
				}
			}
		}

	}
}