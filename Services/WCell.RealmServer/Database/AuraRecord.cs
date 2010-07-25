using Castle.ActiveRecord;
using Cell.Core;
using WCell.Constants.Spells;
using WCell.Core;
using WCell.Core.Database;
using WCell.RealmServer.Global;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells;
using NLog;

namespace WCell.RealmServer.Database
{
	[ActiveRecord(Access = PropertyAccess.Property)]
	public class AuraRecord : WCellRecord<AuraRecord>
	{
		internal static readonly ObjectPool<AuraRecord> AuraRecordPool = new ObjectPool<AuraRecord>(() => new AuraRecord());

		private static readonly NHIdGenerator _idGenerator =
			new NHIdGenerator(typeof(AuraRecord), "RecordId");

		/// <summary>
		/// Returns the next unique Id for a new SpellRecord
		/// </summary>
		public static long NextId()
		{
			return _idGenerator.Next();
		}

		public static AuraRecord ObtainAuraRecord(Aura aura)
		{
			var record = AuraRecordPool.Obtain();
			record.New = true;
			record.RecordId = NextId();
			record.SyncData(aura);

			return record;
		}

		public AuraRecord(Aura aura)
		{
			New = true;
			RecordId = NextId();

			SyncData(aura);
		}

		public AuraRecord()
		{
		}

		public void SyncData(Aura aura)
		{
			OwnerId = aura.Auras.Owner.EntityId.Low;
			CasterId = (long)aura.CasterInfo.CasterId.Full;
			Level = aura.Level;
			m_spell = aura.Spell;
			if (aura.HasTimeout)
			{
				MillisLeft = aura.TimeLeft;
			}
			else
			{
				MillisLeft = -1;
			}
			StackCount = aura.StackCount;
			IsBeneficial = aura.IsBeneficial;
		}

		[Field("OwnerId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int m_OwnerId;

		private Spell m_spell;

		[PrimaryKey(PrimaryKeyType.Assigned)]
		public long RecordId
		{
			get;
			set;
		}

		public uint OwnerId
		{
			get { return (uint)m_OwnerId; }
			set { m_OwnerId = (int)value; }
		}

		[Property]
		public long CasterId
		{
			get;
			set;
		}

		[Property]
		public int Level
		{
			get;
			set;
		}

		[Property]
		public int SpellId
		{
			get { return (int)m_spell.Id; }
			set
			{
				m_spell = SpellHandler.Get((uint)value);
				if (m_spell == null)
				{
					LogManager.GetCurrentClassLogger().Warn("Aura record {0} has invalid SpellId {1}", RecordId, value);
				}
			}
		}

		public Spell Spell
		{
			get { return m_spell; }
		}

		[Property]
		public int MillisLeft
		{
			get;
			set;
		}

		[Property]
		public int StackCount
		{
			get;
			set;
		}

		[Property]
		public bool IsBeneficial
		{
			get;
			set;
		}

		public ObjectInfo GetCasterInfo(Region region)
		{
			var id = new EntityId((ulong)CasterId);
			var caster = region.GetObject(id);
			if (caster != null)
			{
				return caster.CasterInfo;
			}
			return new ObjectInfo(id, Level);
		}

		public override void Delete()
		{
			base.Delete();
			AuraRecordPool.Recycle(this);
		}

		internal void Recycle()
		{
			AuraRecordPool.Recycle(this);
		}
	}
}