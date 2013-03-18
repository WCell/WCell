using System.Collections.Generic;
using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells;
using WCell.Util.Logging;
using WCell.Util.ObjectPools;

namespace WCell.RealmServer.Database.Entities
{
	public class AuraRecord
	{
		internal static readonly ObjectPool<AuraRecord> AuraRecordPool = new ObjectPool<AuraRecord>(() => new AuraRecord());

		public static AuraRecord ObtainAuraRecord(Aura aura)
		{
			var record = AuraRecordPool.Obtain();
			record.SyncData(aura);

			return record;
		}

		public static IEnumerable<AuraRecord> LoadAuraRecords(uint lowId)
		{
            //TODO: Use Detatched Criteria for this
            return RealmWorldDBMgr.DatabaseProvider.Session.QueryOver<AuraRecord>().Where(x => x.OwnerId == (int)lowId).List();
		}

		public AuraRecord(Aura aura)
		{
			SyncData(aura);
		}

		public AuraRecord()
		{
		}

		public void SyncData(Aura aura)
		{
			OwnerId = aura.Auras.Owner.EntityId.Low;
			CasterId = (long)aura.CasterReference.EntityId.Full;
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

		private Spell m_spell;

	    public long RecordId;

	    public uint OwnerId;

		public long CasterId
		{
			get;
			set;
		}

		public int Level
		{
			get;
			set;
		}

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

	    public int MillisLeft;

	    public int StackCount;

	    public bool IsBeneficial;

		public ObjectReference GetCasterInfo(Map map)
		{
			var id = new EntityId((ulong)CasterId);
			var caster = map.GetObject(id);
			if (caster != null)
			{
				return caster.SharedReference;
			}
			return new ObjectReference(id, Level);
		}

        // TODO: integrate this into fluent, under Active Record this was called when the entity was deleted from the database
        //pubic override void Delete()
		public void Delete()
		{
			AuraRecordPool.Recycle(this);
		}

		internal void Recycle()
		{
			AuraRecordPool.Recycle(this);
		}
	}
}