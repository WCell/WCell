using Castle.ActiveRecord;
using WCell.Constants.Spells;
using WCell.Core.Database;

namespace WCell.RealmServer.Database
{
	[ActiveRecord(Access = PropertyAccess.Property)]
	public class SpellRecord : WCellRecord<SpellRecord>
	{
		private static readonly NHIdGenerator _idGenerator =
			new NHIdGenerator(typeof(SpellRecord), "RecordId");

		/// <summary>
		/// Returns the next unique Id for a new SpellRecord
		/// </summary>
		public static long NextId()
		{
			return _idGenerator.Next();
		}

		[Field("SpellId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private long _spellId;

		[Field("OwnerId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int m_OwnerId;

		public SpellRecord(uint id, uint ownerId)
		{
			SpellId = id;
			OwnerId = ownerId;
			RecordId = NextId();
			New = true;
		}

		public SpellRecord()
		{
		}

		[PrimaryKey(PrimaryKeyType.Assigned, "SpellRecordId")]
		public long RecordId
		{
			get;
			set;
		}

		public uint OwnerId
		{
			get { return (uint) m_OwnerId; }
			set { m_OwnerId = (int) value; }
		}

		public uint SpellId
		{
			get { return (uint)_spellId; }
			set { _spellId = value; }
		}

		public override string ToString()
		{
			return (SpellId)_spellId + " (" + SpellId + ")";
		}
	}
}