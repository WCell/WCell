using System;
using WCell.Constants.NPCs;
using WCell.Constants.Pets;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.NPCs.Pets
{
	public abstract class PetRecordBase<R> : IPetRecord
		where R : IPetRecord
	{
		//[Field("OwnerLowId", NotNull = true)]
		protected int _OwnerLowId;

		//[Field("NameTimeStamp")]
		protected int _NameTimeStamp;

		//[Field("PetState", NotNull = true)]
		protected int _PetState;

		//[Field("PetAttackMode", NotNull = true)]
		protected int _petAttackMode;

		//[Field("PetFlags", NotNull = true)]
		protected int _petFlags;

		private uint[] m_ActionButtons;

		//[PrimaryKey(PrimaryKeyType.Assigned, "EntryId")]
		int _EntryId
		{
			get;
			set;
		}

		public uint OwnerId
		{
			get { return (uint)_OwnerLowId; }
			set { _OwnerLowId = (int)value; }
		}

		public NPCId EntryId
		{
			get { return (NPCId)_EntryId; }
			set { _EntryId = (int)value; }
		}

		public virtual uint PetNumber
		{
			get { return 0; }
			set { throw new InvalidOperationException("Cannot set PetNumber"); }
		}

		public NPCEntry Entry
		{
			get { return NPCMgr.GetEntry(EntryId); }
			set { EntryId = value.NPCId; }
		}

		//[Property]
		public bool IsActivePet
		{
			get;
			set;
		}

		//[Property]
		public string Name
		{
			get;
			set;
		}

		public uint NameTimeStamp
		{
			get { return (uint)_NameTimeStamp; }
			set { _NameTimeStamp = (int)value; }
		}

		public PetState PetState
		{
			get { return (PetState)(_PetState); }
			set { _PetState = (int)value; }
		}

		public PetAttackMode AttackMode
		{
			get { return (PetAttackMode)((byte)_petAttackMode); }
			set { _petAttackMode = (int)value; }
		}

		public PetFlags Flags
		{
			get { return (PetFlags)((ushort)_petFlags); }
			set { _petFlags = (int)value; }
		}

		public bool IsStabled
		{
			get { return Flags.HasFlag(PetFlags.Stabled); }
			set
			{
				if (value)
				{
					Flags |= PetFlags.Stabled;
					IsActivePet = false;
				}
				else
				{
					Flags &= ~PetFlags.Stabled;
					IsActivePet = true;
				}
			}
		}

		/// <summary>
		/// Dirty records have uncommitted changes
		/// </summary>
		//public bool IsDirty
		//{
		//	get;
		//	internal set;
		//}

		/// <summary>
		/// 
		/// </summary>
		//[Property(NotNull = true)]
		public uint[] ActionButtons
		{
			get { return m_ActionButtons; }
			set { m_ActionButtons = value; }
		}

		#region Create / Setup / Update
		public void Create()
		{
			RealmWorldDBMgr.DatabaseProvider.Save(this);
			//IsDirty = false;
		}

		public void Update()
		{
			RealmWorldDBMgr.DatabaseProvider.SaveOrUpdate(this);
			//IsDirty = false;
		}

		public void Save()
		{
			RealmWorldDBMgr.DatabaseProvider.SaveOrUpdate(this);
		}

		public void Delete()
		{
			RealmWorldDBMgr.DatabaseProvider.Delete(this);
		}

		public virtual void SetupPet(NPC pet) //TODO: Check this is actually setting / saving the correct values
		{
			if (!string.IsNullOrEmpty(Name))
			{
				pet.SetName(Name, NameTimeStamp);
			}

			pet.PetState = PetState;
			// IsDirty = true;		// save after load?
		}

		public virtual void UpdateRecord(NPC pet)
		{
			if (pet.PetNameTimestamp != 0)
			{
				Name = pet.Name;
				NameTimeStamp = pet.PetNameTimestamp;
			}
			PetState = pet.PetState;
			EntryId = (NPCId)pet.EntryId;
			RealmWorldDBMgr.DatabaseProvider.SaveOrUpdate(this);
			//IsDirty = true;
		}

		#endregion
	}
}