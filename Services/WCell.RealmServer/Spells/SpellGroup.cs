using System.Collections.Generic;
using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells
{
	/// <summary>
	/// A set of Spells
	/// </summary>
	public class SpellGroup : List<Spell>, ISpellGroup
	{
		public void Add(ISpellGroup group)
		{
			foreach (var spell in group)
			{
				base.Add(spell);
			}
		}

		public void Add(params SpellLineId[] ids)
		{
			foreach (var id in ids)
			{
				var line = SpellLines.GetLine(id);
				Add(line);
			}
		}

		public void Add(params SpellId[] ids)
		{
			foreach (var id in ids)
			{
				var spell = SpellHandler.Get(id);
				Add(spell);
			}
		}
	}

	public class AuraCasterGroup : SpellGroup
	{
		public AuraCasterGroup()
			: this(1)
		{
		}

		public AuraCasterGroup(int aurasPerCaster)
		{
			MaxCount = aurasPerCaster;
		}

		/// <summary>
		/// The amount of Auras from this group that one caster may 
		/// apply to a single target.
		/// </summary>
		public int MaxCount
		{
			get;
			set;
		}
	}
}