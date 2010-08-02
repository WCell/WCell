using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Core.Initialization;
using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells
{
	public static partial class SpellLines
	{
		public static readonly SpellLine[][] SpellLinesByClass = new SpellLine[(int)ClassId.End][];
		public static readonly SpellLine[] ById = new SpellLine[(int)SpellLineId.End + 100];

		public static SpellLine GetLine(this SpellLineId id)
		{
			return ById[(int)id];
		}

		public static SpellLine[] GetLines(ClassId clss)
		{
			return SpellLinesByClass[(int)clss];
		}

		internal static void InitSpellLines()
		{
			SetupSpellLines();
		}

		static void AddSpellLines(ClassId clss, SpellLine[] lines)
		{
			SpellLinesByClass[(int)clss] = lines;
			foreach (var line in lines)
			{
				ById[(int)line.LineId] = line;
				Spell last = null;
				foreach (var spell in line)
				{
					if (last != null)
					{
						spell.PreviousRank = last;
						last.NextRank = spell;
					}
					last = spell;
				}
			}
		}
	}
}