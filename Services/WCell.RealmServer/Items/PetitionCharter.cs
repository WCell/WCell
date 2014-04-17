using System.Collections.Generic;
using WCell.Constants;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Items
{
	public class PetitionCharter : Item
	{
		private PetitionRecord _Petition;

		public PetitionCharter()
		{
		}

		protected override void OnLoad()
		{
			_Petition = RealmWorldDBMgr.DatabaseProvider.FindOne<PetitionRecord>(x => x.OwnerId == Owner.EntityId.Low);
		}

        protected internal override void DoDestroy()
        {
            RealmWorldDBMgr.DatabaseProvider.Delete<PetitionRecord>(x => x.OwnerId == Owner.EntityId.Low);
            base.DoDestroy();
        }

		public PetitionRecord Petition
		{
			get { return _Petition; }
            set { _Petition = value; }
		}
	}

	public class PetitionRecord
	{
        public PetitionRecord()
        {
        }

		public PetitionRecord(string name, uint ownerId, uint itemId, PetitionType type)
		{
            Name = name;
			OwnerId = ownerId;
			ItemId = (int)itemId;
			SignedIds = new List<uint>(9);
            Type = type;
		}

		public int ItemId
		{
			get;
			set;
		}

        public string Name
        {
            get;
            set;
        }

        public uint OwnerId
        {
            get;
            set;
        }

        public PetitionType Type
        {
            get;
            set;
        }

		public IList<uint> SignedIds
		{
			get;
			set;
		}

		public void AddSignature(uint signedId)
		{
			SignedIds.Add(signedId);
            RealmWorldDBMgr.DatabaseProvider.Update<PetitionRecord>(this);
		}

		public static PetitionRecord LoadRecord(int ownerId)
		{
			return RealmWorldDBMgr.DatabaseProvider.FindOne<PetitionRecord>(x => x.OwnerId == ownerId);
		}

        public static bool CanBuyPetition(uint ownerId)
        {
            return !RealmWorldDBMgr.DatabaseProvider.Exists<PetitionRecord>(x => x.OwnerId == ownerId);
        }

        public static PetitionRecord LoadRecordByItemId(uint itemId)
        {
            return RealmWorldDBMgr.DatabaseProvider.FindFirst<PetitionRecord>(x => x.ItemId == itemId);
        }
	}
}