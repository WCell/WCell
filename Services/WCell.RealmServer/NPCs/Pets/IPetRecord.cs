using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.NPCs;
using WCell.Constants.Pets;
using PetAttackMode=WCell.Constants.Pets.PetAttackMode;
using PetFlags=WCell.Constants.Pets.PetFlags;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.NPCs.Pets
{
	public interface IPetRecord
	{
		uint OwnerId
		{
			get;
			set;
		}

		NPCId EntryId
		{
			get;
			set;
		}

		uint PetNumber
		{
			get;
			set;
		}

		NPCEntry Entry
		{
			get;
			set;
		}

		bool IsActivePet
		{
			get;
			set;
		}

		string Name
		{
			get;
			set;
		}

		uint NameTimeStamp
		{
			get;
			set;
		}

		PetState PetState
		{
			get;
			set;
		}

		PetAttackMode AttackMode
		{
			get;
			set;
		}

		PetFlags Flags
		{
			get;
			set;
		}

		bool IsStabled
		{
			get;
			set;
		}

		bool IsDirty
		{
			get;
		}

		uint[] ActionButtons
		{
			get;
			set;
		}

		void Save();

		void Delete();

		void SetupPet(NPC pet);

		void UpdateRecord(NPC pet);
	}
}