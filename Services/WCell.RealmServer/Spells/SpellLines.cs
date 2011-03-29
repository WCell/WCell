using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Core.Initialization;
using WCell.Constants.Spells;
using WCell.Util.Logging;

namespace WCell.RealmServer.Spells
{
	public static partial class SpellLines
	{
		public static readonly SpellLine[][] SpellLinesByClass = new SpellLine[(int)ClassId.End][];
		public static readonly SpellLine[] ById = new SpellLine[(int)SpellLineId.End + 100];
	    public static Logger log = LogManager.GetCurrentClassLogger();

		public static SpellLine GetLine(this SpellLineId id)
		{
			return ById[(int)id];
		}

		public static SpellLine[] GetLines(ClassId clss)
		{
            if((int)clss >= SpellLinesByClass.Length)
            {
                log.Error("Class out of range of SpellLine array {1}:{0} SpellLinesByClass.Length {2}", clss, (int)clss,
                              SpellLinesByClass.Length);
                return null;
            }
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