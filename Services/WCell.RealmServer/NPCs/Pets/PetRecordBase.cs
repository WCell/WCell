using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.ActiveRecord;
using WCell.Constants;
using WCell.Constants.NPCs;
using WCell.Constants.Pets;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.NPCs.Pets
{
	public abstract class PetRecordBase<R> : ActiveRecordBase<R>, IPetRecord
		where R : IPetRecord
	{
		[Field("OwnerLowId", NotNull = true)]
		protected int _OwnerLowId;

		[Field("EntryId", NotNull = true)]
		protected int _EntryId;

		[Field("NameTimeStamp")]
		protected int _NameTimeStamp;

		[Field("PetState", NotNull = true)]
		protected int _PetState;

		[Field("PetAttackMode", NotNull = true)]
		protected int _petAttackMode;

		[Field("PetFlags", NotNull = true)]
		protected int _petFlags;

		private PetActionEntry[] m_Actions;
		private uint[] m_savedActions;

		[PrimaryKey(PrimaryKeyType.Native)]
		int Guid
		{
			get;
			set;
		}

		public uint OwnerId
		{
			get { return (uint)_OwnerLowId; }
			set { _OwnerLowId = (int)value; }
		}

	    public virtual PetType Type
	    {
	        get { return PetType.None; }
	        set { }
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

		[Property]
		public bool IsActivePet
		{
			get;
			set;
		}

		[Property]
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

		/// <summary>
		/// 
		/// </summary>
		[Property(NotNull = true)]
		public uint[] SavedActions
		{
			get { return m_savedActions; }
			set
			{
				m_savedActions = value;
				m_Actions = new PetActionEntry[value.Length];
				for (var i = 0; i < value.Length; i++)
				{
					var action = value[i];
					m_Actions[i] = action;
				}
			}
		}

		public PetActionEntry[] Actions
		{
			get { return m_Actions; }
			set
			{
				m_Actions = value;
				CopySavedActions();
			}
		}

		void CopySavedActions()
		{
			if (m_savedActions == null)
			{
				m_savedActions = new uint[m_Actions.Length];
			}
			for (var i = 0; i < m_Actions.Length; i++)
			{
				var action = m_Actions[i];
				m_savedActions[i] = action;
			}
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
			get { return Flags.Has(PetFlags.Stabled); }
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
		public bool IsDirty
		{
			get;
			internal set;
		}

		public override void Save()
		{
			base.Save();
			IsDirty = false;
		}

		public override void Update()
		{
			base.Update();
			IsDirty = false;
		}

		public virtual void SetupPet(NPC pet)
		{
			if (!string.IsNullOrEmpty(Name))
			{
				pet.SetName(Name, NameTimeStamp);
			}

			pet.PetState = PetState;
			IsDirty = true;
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
			CopySavedActions();
			IsDirty = true;
		}
	}
}
