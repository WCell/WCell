using System;
using WCell.Constants.Pets;
using WCell.RealmServer.Database;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Database.Entities.Pets
{
	public class PetSpell
	{
		public static readonly PetSpell[] EmptyArray = new PetSpell[0];

		public long Guid
		{
			get;
			set;
		}

		private Spell m_Spell;

		public Spell Spell
		{
			get { return m_Spell; }
			set { m_Spell = value; }
		}

		public int SpellId
		{
			get { return m_Spell != null ? (int)m_Spell.Id : 0; }
			set { m_Spell = SpellHandler.Get((uint)value); }
		}

		private int _petSpellState;

		public PetSpellState State
		{
			get
			{
				return (PetSpellState)((ushort)_petSpellState);
			}
			set
			{
				_petSpellState = (int)value;
			}
		}

		public override string ToString()
		{
			return Spell.ToString();
		}
	}
}