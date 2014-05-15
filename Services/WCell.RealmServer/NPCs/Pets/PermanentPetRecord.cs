using System;
using System.Collections.Generic;
using System.Linq;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.NPCs.Pets
{
	/// <summary>
	/// Record for Hunter pets with talents and everything
	/// </summary>
	//[ActiveRecord("Pets_Permanent", Access = PropertyAccess.Property)]
	public class PermanentPetRecord : PetRecordBase<PermanentPetRecord>
	{
		public PermanentPetRecord()
		{
		}

		//public EntityId EntityId
		//{
		//    get;
		//    private set;
		//}

		//[Field("PetNumber", NotNull = true)]
		private int m_PetNumber;

		public override uint PetNumber
		{
			get { return (uint)m_PetNumber; }
			set { m_PetNumber = (int)value; }
		}

		//[Property]
		public int Experience
		{
			get;
			set;
		}

		//[Property]
		public int Level
		{
			get;
			set;
		}

		//[Property]
		public DateTime? StabledSince
		{
			get;
			set;
		}

		//[Property]
		public DateTime? LastTalentResetTime
		{
			get;
			set;
		}

		//[Property]
		public int TalentResetPriceTier
		{
			get;
			set;
		}

		//[Property(NotNull = true)]
		public int FreeTalentPoints
		{
			get;
			set;
		}

		/// <summary>
		///
		/// </summary>
		public IList<PetTalentSpellRecord> Spells
		{
			get;
			set;
		}

		public override void SetupPet(NPC pet)
		{
			base.SetupPet(pet);

			pet.PetExperience = Experience;
			pet.Level = Level;
			pet.LastTalentResetTime = LastTalentResetTime;
			if (pet.HasTalents)
			{
				pet.FreeTalentPoints = FreeTalentPoints;
			}
		}

		public override void UpdateRecord(NPC pet)
		{
			base.UpdateRecord(pet);

			Experience = pet.PetExperience;
			PetNumber = pet.PetNumber;
			Entry = pet.Entry;
			Level = pet.Level;
			LastTalentResetTime = pet.LastTalentResetTime;
			if (pet.HasTalents)
			{
				FreeTalentPoints = pet.FreeTalentPoints;
			}
		}

		public static PermanentPetRecord[] LoadPermanentPetRecords(uint ownerId)
		{
			try
			{
				return RealmWorldDBMgr.DatabaseProvider.Query<PermanentPetRecord>().Where(permanentPetRecord => permanentPetRecord.OwnerId == ownerId).ToArray(); //FindAllByProperty("_OwnerLowId", (int)ownerId);
			}
			catch (Exception e)
			{
				RealmWorldDBMgr.OnDBError(e);
				return RealmWorldDBMgr.DatabaseProvider.Query<PermanentPetRecord>().Where(permanentPetRecord => permanentPetRecord.OwnerId == ownerId).ToArray();  //FindAllByProperty("_OwnerLowId", (int)ownerId);
			}
		}
	}
}