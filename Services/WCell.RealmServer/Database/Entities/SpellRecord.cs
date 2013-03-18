using System.Collections.Generic;
using WCell.Constants.Spells;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Database.Entities
{
	public class SpellRecord
	{
		public const int NoSpecIndex = -1;


		public static IEnumerable<SpellRecord> LoadAllRecordsFor(uint lowId)
		{
            //TODO: Use Detatched Criteria for this
			return RealmWorldDBMgr.DatabaseProvider.Session.QueryOver<SpellRecord>().Where(x => x.OwnerId == (int)lowId).List();
		}

		private SpellRecord()
		{
		}

		public SpellRecord(SpellId id, uint ownerId, int specIndex)
		{
			SpellId = id;
			OwnerId = ownerId;
			SpecIndex = specIndex;
		}

	    public long RecordId;

	    public uint OwnerId;

	    public SpellId SpellId;

		public Spell Spell
		{
			get { return SpellHandler.Get(SpellId); }
		}

	    public int SpecIndex;

		public bool MatchesSpec(int index)
		{
			return SpecIndex == index || index == NoSpecIndex;
		}

		public override string ToString()
		{
			return Spell + " (" + SpellId + ")";
		}
	}
}