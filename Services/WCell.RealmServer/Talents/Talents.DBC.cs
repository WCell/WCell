/*************************************************************************
 *
 *   file		: Talents.DBC.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-12-23 20:07:17 +0100 (on, 23 dec 2009) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1151 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.Collections.Generic;
using NLog;
using WCell.Constants;
using WCell.Constants.Talents;
using WCell.Core;
using WCell.Core.DBC;
using WCell.RealmServer.Spells;
using WCell.Util;

namespace WCell.RealmServer.Talents
{
	public static partial class TalentMgr
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		#region TalentTab.dbc
		public class TalentTreeConverter : AdvancedDBCRecordConverter<TalentTree>
		{
			public override TalentTree ConvertTo(byte[] rawData, ref int id)
			{
				var tree = new TalentTree();

				id = (int)(tree.Id = (TalentTreeId)GetUInt32(rawData, 0));
				tree.Name = GetString(rawData, 1);

				var classMask = (ClassMask)GetUInt32(rawData, 20);
				tree.Class = WCellDef.ClassTypesByMask[classMask];

				tree.TabIndex2 = GetUInt32(rawData, 21);
				tree.TabIndex = GetUInt32(rawData, 22);


				return tree;
			}
		}
		#endregion


		#region Talent.dbc
		public class TalentConverter : AdvancedDBCRecordConverter<TalentEntry>
		{
			public override TalentEntry ConvertTo(byte[] rawData, ref int id)
			{
				var talent = new TalentEntry();

				id = (int)(talent.Id = (TalentId)GetUInt32(rawData, 0));

				var treeId = (TalentTreeId)GetUInt32(rawData, 1);
				talent.Tree = TalentTrees.Get((uint)treeId);

				if (talent.Tree == null)
				{
					return null;
				}

				talent.Row = GetUInt32(rawData, 2);
				talent.Col = GetUInt32(rawData, 3);

				var abilities = new List<Spell>(5);
				uint spellId;

				for (int i = 0; i < 9; i++)
				{
					spellId = GetUInt32(rawData, i + 4);

					// There are talents linking to invalid spells, eg Dirty Tricks, Rank 3
					Spell spell;
					if (spellId == 0 || (spell = SpellHandler.Get(spellId)) == null)
					{
						break;
					}
					if (spell.IsTeachSpell)
					{
						spell = spell.GetEffectsWhere(effect => effect.TriggerSpell != null)[0].TriggerSpell;
					}

					if (spell != null)
					{
						abilities.Add(spell);
					}
					else
					{
						log.Warn("Talent has invalid Spell: {0} ({1})", talent.Id, spellId);
					}
				}
				talent.Spells = abilities.ToArray();

				talent.RequiredId = (TalentId)GetUInt32(rawData, 13);
				talent.RequiredRank = GetUInt32(rawData, 16);

				return talent;
			}
		}
		#endregion
	}
}