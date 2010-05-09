using System;
using Castle.ActiveRecord;
using WCell.Constants.Pets;
using WCell.RealmServer.Database;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.NPCs.Pets
{
	public class PetSpell //: ActiveRecordBase<PetSpell>
	{
		public static readonly PetSpell[] EmptyArray = new PetSpell[0];

		[PrimaryKey(PrimaryKeyType.GuidComb, "PetSpellId")]
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

		[Property("SpellId", NotNull = true)]
		public int SpellId
		{
			get { return m_Spell != null ? (int)m_Spell.Id : 0; }
			set { m_Spell = SpellHandler.Get((uint)value); }
		}

		[Field("PetSpellState", NotNull = true)]
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