using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.ActiveRecord;
using WCell.Constants;
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
			// existing item: Load petition info from DB
			// TODO: Consider when to load from DB
			m_Petition = PetitionRecord.Find((int)EntityId.Low);
		}

		public PetitionRecord Petition
		{
			get { return m_Petition; }
		}
	}

	[ActiveRecord(Access = PropertyAccess.Property)]
	public class PetitionRecord : ActiveRecordBase<PetitionRecord>
	{
		[Field(NotNull = true)]
		private int m_OwnerId;

		public PetitionRecord(uint ownerId, uint itemId)
		{
			OwnerId = ownerId;
			ItemId = (int) itemId;
			SignedIds = new List<int>(3);
		}

		[PrimaryKey(PrimaryKeyType.Assigned)]
		public int ItemId
		{
			get;
			private set;
		}

		public uint OwnerId
		{
			get { return (uint)m_OwnerId; }
			set { m_OwnerId = (int)value; }
		}

		[Property(NotNull = true)]
		public PetitionType Type
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public List<int> SignedIds
		{
			get;
			set;
		}

		public void AddSignature(uint signedId)
		{
			SignedIds.Add((int)signedId);
		}

		public static PetitionRecord LoadRecord(uint itemId)
		{
			return Find((int)itemId);
		}
	}
}