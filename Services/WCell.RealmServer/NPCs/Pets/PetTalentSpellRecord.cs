using System;
using Castle.ActiveRecord;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.NPCs.Pets
{
	[ActiveRecord("PetTalentSpellRecords", Access = PropertyAccess.Property)]
	public class PetTalentSpellRecord : ActiveRecordBase<PetTalentSpellRecord>
	{
		[Field("SpellId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private long _spellId;

		[PrimaryKey(PrimaryKeyType.Native, "TalentRecordId")]
		public long RecordId
		{
			get;
			set;
		}

		[Property("CooldownUntil", NotNull = true)]
		public DateTime? CooldownUntil
		{
			get;
			set;
		}

		public uint SpellId
		{
			get { return (uint)_spellId; }
			set { _spellId = value; }
		}

		public Spell Spell
		{
			get { return SpellHandler.ById[_spellId]; }
			set { _spellId = (long)value.SpellId; }
		}

	}
}