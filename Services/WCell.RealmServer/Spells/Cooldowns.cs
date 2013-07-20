using System;

namespace WCell.RealmServer.Spells
{
	public interface ICooldown
	{
		DateTime Until { get; set; }
		IConsistentCooldown AsConsistent();
	}

	public interface IConsistentCooldown : ICooldown
	{
		uint CharId { get; set; }

		void Save();
		void Update();
		void Create();
		void Delete();
	}

	public interface ISpellIdCooldown : ICooldown
	{
		uint SpellId { get; set; }
		uint ItemId { get; set; }
	}

	public interface ISpellCategoryCooldown : ICooldown
	{
		uint SpellId { get; set; }
		uint CategoryId { get; set; }
		uint ItemId { get; set; }
	}

	public class SpellIdCooldown : ISpellIdCooldown
	{
		public DateTime Until
		{
			get;
			set;
		}
		public uint SpellId
		{
			get;
			set;
		}
		public uint ItemId
		{
			get;
			set;
		}

		public IConsistentCooldown AsConsistent()
		{
			return new PersistentSpellIdCooldown
			{
				Until = Until,
				SpellId = SpellId,
				ItemId = ItemId
			};
		}
	}

	public class SpellCategoryCooldown : ISpellCategoryCooldown
	{
		public DateTime Until
		{
			get;
			set;
		}
		public uint SpellId
		{
			get;
			set;
		}
		public uint CategoryId
		{
			get;
			set;
		}
		public uint ItemId
		{
			get;
			set;
		}

		public IConsistentCooldown AsConsistent()
		{
			return new PersistentSpellCategoryCooldown
			{
				Until = Until,
				CategoryId = CategoryId,
				ItemId = ItemId
			};
		}
	}

	[ActiveRecord("SpellIdCooldown", Access = PropertyAccess.Property)]
	public class PersistentSpellIdCooldown : ActiveRecordBase<PersistentSpellIdCooldown>, ISpellIdCooldown, IConsistentCooldown
	{
		public static PersistentSpellIdCooldown[] LoadIdCooldownsFor(uint lowId)
		{
			return FindAllByProperty("_charId", (int)lowId);
		}

		[Field("SpellId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int _spellId;
		[Field("ItemId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int _itemId;
		[Field("CharId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int _charId;

		[PrimaryKey(PrimaryKeyType.Increment)]
		private long Id
		{
			get;
			set;
		}

		public uint CharId
		{
			get { return (uint)_charId; }
			set { _charId = (int)value; }
		}

		[Property]
		public DateTime Until
		{
			get;
			set;
		}

		public uint SpellId
		{
			get { return (uint)_spellId; }
			set { _spellId = (int)value; }
		}

		public uint ItemId
		{
			get { return (uint)_itemId; }
			set { _itemId = (int)value; }
		}

		public IConsistentCooldown AsConsistent()
		{
			return this;
		}
	}

	[ActiveRecord("SpellCategoryCooldown", Access = PropertyAccess.Property)]
	public class PersistentSpellCategoryCooldown : ActiveRecordBase<PersistentSpellCategoryCooldown>, ISpellCategoryCooldown, IConsistentCooldown
	{
		public static PersistentSpellCategoryCooldown[] LoadCategoryCooldownsFor(uint lowId)
		{
			return FindAllByProperty("_charId", (int)lowId);
		}

		[Field("CatId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int _catId;
		[Field("ItemId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int _itemId;
		[Field("CharId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int _charId;
		[Field("SpellId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int _spellId;

		[PrimaryKey(PrimaryKeyType.Increment)]
		private long Id
		{
			get;
			set;
		}

		public uint SpellId
		{
			get { return (uint)_spellId; }
			set { _spellId = (int)value; }
		}

		public uint CharId
		{
			get { return (uint)_charId; }
			set { _charId = (int)value; }
		}

		[Property]
		public DateTime Until
		{
			get;
			set;
		}

		public uint CategoryId
		{
			get { return (uint)_catId; }
			set { _catId = (int)value; }
		}

		public uint ItemId
		{
			get { return (uint)_itemId; }
			set { _itemId = (int)value; }
		}

		public IConsistentCooldown AsConsistent()
		{
			return this;
		}
	}
}