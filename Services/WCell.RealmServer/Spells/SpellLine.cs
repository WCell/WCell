using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Util;

namespace WCell.RealmServer.Spells
{
	/// <summary>
	/// SpellLines are holding class-spells of different Ranks together
	/// </summary>
	public class SpellLine : ISpellGroup
	{
		private readonly List<Spell> Spells;
		private readonly Spell m_firstSpell;

		public readonly SpellLineId LineId;

		public SpellLine(SpellLineId id, params Spell[] spells)
		{
			LineId = id;
			AuraUID =  (uint)id;
			Spells = new List<Spell>();
			if (spells.Length > 0)
			{
				m_firstSpell = spells[0];

				for (var i = 0; i < spells.Length; i++)
				{
					var spell = spells[i];
					if (spell != null)
					{
						AddSpell(spell);
					}
				}
			}
		}

		public string Name
		{
			get { return GetSpellLineName(m_firstSpell); }
		}

		public ClassId ClassId
		{
			get { return m_firstSpell.ClassId; }
		}

		public Spell FirstRank
		{
			get { return m_firstSpell; }
		}

		/// <summary>
		/// The spell with the highest rank in this line
		/// </summary>
		public Spell HighestRank
		{
			get
			{
				var spell = m_firstSpell;
				while (spell.NextRank != null)
				{
					spell = spell.NextRank;
				}
				return spell;
			}
		}

		public uint AuraUID
		{
			get;
			internal set;
		}

		internal void AddSpell(Spell spell)
		{
			if (spell == null)
			{
				throw new ArgumentNullException("spell");
			}
			Spells.Add(spell);
			spell.Line = this;
		}

		public IEnumerator<Spell> GetEnumerator()
		{
			return Spells.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public static SpellLineId GetSpellLineId(Spell spell)
		{
			return EnumUtil.Parse<SpellLineId>(GetSpellLineName(spell));
		}

		public static string GetSpellLineName(Spell spell)
		{
			var name = spell.SpellId.ToString().Replace("ClassSkill", "").Replace("Rank", "");

			var len = name.Length;

			char c;
			while (Char.IsDigit(c = name[len - 1]) || c == '_')
			{
				len--;
			}

			name = name.Substring(0, len);
			if (spell.ClassId != 0 && !name.StartsWith(spell.ClassId.ToString()))
			{
				name = spell.ClassId + name;
			}
			return name;
		}

		public override string ToString()
		{
			return Name;
		}

		public Spell BaseSpell
		{
			get { return FirstRank; }
		}
	}
}
