using System.Collections.Generic;
using Castle.ActiveRecord;
using WCell.Constants;
using WCell.Core.Database;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Items
{
    public class PetitionCharter : Item
    {
        private PetitionRecord m_Petition;

        public PetitionCharter()
        {
        }

        protected override void OnLoad()
        {
            m_Petition = PetitionRecord.Find((int)Owner.EntityId.Low);
        }

        protected internal override void DoDestroy()
        {
            Petition.Delete();
            base.DoDestroy();
        }

        public PetitionRecord Petition
        {
            get { return m_Petition; }
            set { m_Petition = value; }
        }
    }

    [ActiveRecord("PetitionRecord", Access = PropertyAccess.Property)]
    public class PetitionRecord : WCellRecord<PetitionRecord>
    {
        [Field("Type", NotNull = true)]
        private int m_Type;

        [PrimaryKey(PrimaryKeyType.Assigned, "OwnerId")]
        private int m_OwnerId
        {
            get;
            set;
        }

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

        [Property("ItemId", NotNull = true)]
        private int ItemId
        {
            get;
            set;
        }

        [Property("Name", NotNull = true, Unique = true)]
        public string Name
        {
            get;
            set;
        }

        public uint OwnerId
        {
            get { return (uint)m_OwnerId; }
            set { m_OwnerId = (int)value; }
        }

        public PetitionType Type
        {
            get { return (PetitionType)m_Type; }
            set { m_Type = (int)value; }
        }

        [Property("SignedIds", NotNull = true)]
        public List<uint> SignedIds
        {
            get;
            set;
        }

        public void AddSignature(uint signedId)
        {
            SignedIds.Add(signedId);
            Update();
        }

        public static PetitionRecord LoadRecord(int ownerId)
        {
            return Find(ownerId);
        }

        public static bool CanBuyPetition(uint ownerId)
        {
            if (Exists((int)ownerId))
                return false;
            else
                return true;
        }

        public static PetitionRecord LoadRecordByItemId(uint itemId)
        {
            var property = FindAllByProperty("ItemId", (int)itemId);
            return property[0];
        }
    }
}