using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.GameObjects;
using WCell.Core;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.NPCs.Pets;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Database;
using WCell.Constants.NPCs;
using WCell.Constants.Pets;
using WCell.Util.Graphics;
using Castle.ActiveRecord;

namespace WCell.RealmServer.Entities
{
	public partial class Character
	{
		private List<PermanentPetRecord> m_StabledPetRecords;
		private List<SummonedPetRecord> m_SummonedPetRecords;
		private List<GameObject> m_ownedGOs;
		private int m_petBonusTalentPoints;
		protected NPC m_activePet;
		protected NPCCollection m_minions;
		protected NPC[] m_totems;

		/// <summary>
		/// Currently active Pet of this Character
		/// </summary>
		public NPC ActivePet
		{
			get { return m_activePet; }
			set
			{
				if (value == m_activePet) return;

				if (m_activePet != null)
				{
					LosePet();
				}

				if (IsPetActive = value != null)
				{
					value.PetRecord.IsActivePet = true;
					m_record.PetEntryId = value.Entry.NPCId;

					if (value.SpecProfile != null)
					{
						TalentHandler.SendTalentGroupList(value);
					}

					m_activePet = value;

					AddPostUpdateMessage(() =>
					{
						if (m_activePet == value)
						{
							PetHandler.SendSpells(this, m_activePet, PetAction.Follow);
							PetHandler.SendPetGUIDs(this);
						}
					});
				}
				else
				{
					UnsetActivePet();
				}
			}
		}

		/// <summary>
		/// Action-information of previously summoned pets
		/// </summary>
		public List<SummonedPetRecord> SummonedPetRecords
		{
			get
			{
				if (m_SummonedPetRecords == null)
				{
					m_SummonedPetRecords = new List<SummonedPetRecord>();
				}
				return m_SummonedPetRecords;
			}
		}

		/// <summary>
		/// All minions that belong to this Character, excluding actual Pets.
		/// Might be null.
		/// </summary>
		public NPCCollection Minions
		{
			get { return m_minions; }
		}

		/// <summary>
		/// All created Totems (might be null)
		/// </summary>
		public NPC[] Totems
		{
			get { return m_totems; }
		}

		/// <summary>
		/// Lets the ActivePet appear/disappear (if this Character has one)
		/// </summary>
		public bool IsPetActive
		{
			get { return m_record.IsPetActive; }
			set
			{
				if (value)
				{
					if (m_activePet != null && !m_activePet.IsInWorld)
					{
						// Pet appears
						PlaceOnTop(ActivePet);
					}
				}
				else
				{
					// Pet disappears
					m_activePet.RemoveFromRegion();
				}
				m_record.IsPetActive = value;
			}
		}

		#region Taming and Spawning Pets
		public int StableSlotCount
		{
			get { return Record.StableSlotCount; }
			set { Record.StableSlotCount = value; }
		}

		public int TotalPetCount
		{
			get { return (m_activePet != null ? 1 : 0) + (m_StabledPetRecords != null ? m_StabledPetRecords.Count : 0); }
		}

		public bool HasStabledPets
		{
			get { return m_StabledPetRecords != null && m_StabledPetRecords.Count > 0; }
		}

		public List<PermanentPetRecord> StabledPetRecords
		{
			get
			{
				if (m_StabledPetRecords == null)
				{
					m_StabledPetRecords = new List<PermanentPetRecord>(PetConstants.MaxStableSlots);
				}
				return m_StabledPetRecords;
			}
		}

		/// <summary>
		/// Indicate whether the Character can get a PetSpell bar to
		/// control the given NPC
		/// </summary>
		/// <param name="npc"></param>
		/// <returns></returns>
		public bool CanControl(NPCEntry npc)
		{
			return npc.IsTamable && (!npc.IsExoticPet || CanControlExoticPets);
		}

		public NPC SpawnPet(IPetRecord record)
		{
			return SpawnPet(record, ref m_position, 0);
		}

		public NPC SpawnPet(IPetRecord record, int duration)
		{
			return SpawnPet(record, ref m_position, duration);
		}

		/// <summary>
		/// Tries to spawn a new summoned Pet for the Character.
		/// </summary>
		/// <param name="entry"></param>
		/// <param name="position"></param>
		/// <returns>null, if the Character already has that kind of Pet.</returns>
		public NPC SpawnPet(NPCEntry entry, ref Vector3 position, int durationMillis)
		{
			return SpawnPet(GetOrCreateSummonedPetRecord(entry), ref position, durationMillis);
		}

		public NPC SpawnPet(IPetRecord record, ref Vector3 position, int duration)
		{
			var pet = CreateMinion(record.Entry, duration);
			pet.PetRecord = record;
			pet.Position = position;
			if (record.PetNumber != 0)
			{
				//pet.PetNumber = record.PetNumber;
				pet.EntityId = new EntityId(NPCMgr.GenerateUniqueLowId(), record.PetNumber, HighId.UnitPet);
			}

			OnNewPet(pet);
			if (IsPetActive)
			{
				m_region.AddObject(pet);
			}
			return pet;
		}

		/// <summary>
		/// Makes the given NPC your Pet or Companion
		/// </summary>
		/// <param name="minion">NPC to control</param>
		/// <param name="duration">The amount of time, in miliseconds, to control the minion. 0 is infinite.</param>
		public void MakePet(NPC minion)
		{
			MakePet(minion, 0);
		}

		/// <summary>
		/// Makes the given NPC your Pet or Companion
		/// </summary>
		/// <param name="minion">NPC to control</param>
		/// <param name="duration">The amount of time, in miliseconds, to control the minion. 0 is infinite.</param>
		public void MakePet(NPC minion, int durationMillis)
		{
			Enslave(minion, durationMillis);

			minion.PetRecord = PetMgr.CreatePermanentPetRecord(minion.Entry, m_record.EntityLowId);
			OnNewPet(minion);

			// If a hunter tames a pet that is more than five levels beneath their own 
			// level, the pet will then have their level increased to five levels beneath 
			// the hunter’s own level. 
			if ((Level - minion.Level) > 5)
			{
				var targetLevel = Level - 5;
				while (minion.Level < targetLevel)
				{
					minion.PetExperience = minion.PetNextLevelExp;
					minion.TryPetLevelUp();
				}
			}
		}

		void OnNewPet(NPC pet)
		{
			Summon = pet.EntityId;
			pet.Summoner = this;
			pet.Creator = EntityId;
			pet.PetRecord.SetupPet(pet);
			pet.SetPetAttackMode(pet.PetRecord.AttackMode);

			if (pet.PetRecord is SummonedPetRecord)
			{
				m_record.PetSummonedCount++;
			}
			else
			{
				m_record.PetCount++;
			}

			ActivePet = pet;
		}
		#endregion

		/// <summary>
		/// Dismisses (or deletes entirely) the current pet
		/// </summary>
		public void DismissPet()
		{
			if (m_activePet == null) return;
			if (m_activePet.IsSummoned)
			{
				// delete entirely
				ActivePet = null;
			}
			else
			{
				// just set inactive
				IsPetActive = false;
			}
		}

		private void LosePet()
		{
			if (m_activePet.IsInWorld && m_activePet == m_charm && !m_activePet.PetRecord.IsStabled)
			{
				m_activePet.RejectMaster();
				m_activePet.IsDecaying = true;
			}
			else
			{
				m_activePet.Delete();
				// TODO: loose happiness
			}
			m_record.PetCount--;
		}

		/// <summary>
		/// Simply unsets the currently active pet without deleting or abandoning it.
		/// Make sure to take care of the pet when calling this method.
		/// </summary>
		public void UnsetActivePet()
		{
			if (m_activePet != null)
			{
				if (m_activePet.IsInWorld)
				{
					m_activePet.Summoner = null;
				}
				Summon = EntityId.Zero;
				if (Charm == m_activePet)
				{
					Charm = null;
				}
				m_record.PetEntryId = 0;
				PetHandler.SendEmptySpells(this);
				m_activePet = null;
			}
		}

		#region Stabling
		public PermanentPetRecord GetStabledPet(uint petNumber)
		{
			if (m_StabledPetRecords == null)
			{
				return null;
			}

			foreach (var pet in m_StabledPetRecords)
			{
				if (pet.PetNumber == petNumber)
				{
					return pet;
				}
			}
			return null;
		}

		public PermanentPetRecord GetStabledPetBySlot(uint stableSlot)
		{
			if (m_StabledPetRecords == null || stableSlot > m_StabledPetRecords.Count)
			{
				return null;
			}
			return m_StabledPetRecords[(int)stableSlot];
		}

		/// <summary>
		/// Stable the currently ActivePet.
		/// Make sure there is at least one free StableSlot
		/// </summary>
		/// <returns>True if the pet was stabled.</returns>
		public void StablePet()
		{
			var pet = ActivePet;
			StabledPetRecords.Add(pet.PermanentPetRecord);
			pet.PermanentPetRecord.StabledSince = DateTime.Now;

			// Always set the stabled flag before changing the ActivePet 
			// or the pet will be deleted!
			pet.PetRecord.IsStabled = true;

			ActivePet = null;
		}

		/// <summary>
		/// Tries to make the stabled pet the ActivePet
		/// </summary>
		/// <param name="stabledPermanentPet">The stabled pet to make Active.</param>
		/// <returns>True if the stabled was made Active.</returns>
		public void DeStablePet(PermanentPetRecord stabledPermanentPet)
		{
			m_StabledPetRecords.Remove(stabledPermanentPet);
			var pet = SpawnPet(stabledPermanentPet);
			pet.PermanentPetRecord.StabledSince = null;
		}

		/// <summary>
		/// Tries to swap the ActivePet for a Stabled one.
		/// </summary>
		/// <param name="stabledPermanentPet">The stabled pet to swap out.</param>
		/// <returns>True if the Stabled Pet was swapped.</returns>
		public bool TrySwapStabledPet(PermanentPetRecord stabledPermanentPet)
		{
			if (StabledPetRecords.Count >= (StableSlotCount + 1))
			{
				return false;
			}

			var pet = m_activePet;
			if (pet == null)
			{
				return false;
			}

			var record = pet.PetRecord as PermanentPetRecord;
			if (record == null)
			{
				return false;
			}

			// Always set the stabled flag before changing the ActivePet 
			// or the pet will be deleted!
			record.IsStabled = true;
			StabledPetRecords.Add(record);
			DeStablePet(stabledPermanentPet);
			return true;
		}

		/// <summary>
		/// Tries to have this Character purchase another Stable Slot.
		/// </summary>
		/// <returns>True if successful.</returns>
		public bool TryBuyStableSlot()
		{
			if (StableSlotCount >= PetConstants.MaxStableSlots)
			{
				return false;
			}

			var price = PetMgr.StableSlotPrices[StableSlotCount];
			if (Money < price)
			{
				return false;
			}

			Money -= price;
			StableSlotCount++;
			return true;
		}
		#endregion

		#region Serialization
		private void LoadPets()
		{
			IPetRecord activePetRecord = null;

			// load info for summoned pets
			if (m_record.PetSummonedCount > 0)
			{
				var pets = SummonedPetRecord.LoadSummonedPetRecords(m_record.EntityLowId);
				foreach (var pet in pets)
				{
					if (pet.IsActivePet)
					{
						activePetRecord = pet;
					}
					SummonedPetRecords.Add(pet);
				}
			}

			// load info for permanent pets
			if (m_record.PetCount > 0)
			{
				var pets = PermanentPetRecord.LoadPermanentPetRecords(m_record.EntityLowId);
				foreach (var pet in pets)
				{
					if (pet.IsActivePet)
					{
						activePetRecord = pet;
					}
					else
					{
						StabledPetRecords.Add(pet);
					}
				}
			}


			// load active pet
			if (m_record.PetEntryId != 0 && IsPetActive)
			{
				if (activePetRecord != null)
				{
					var entry = activePetRecord.Entry;
					if (entry == null)
					{
						log.Warn("{0} has invalid PetEntryId: {1} ({2})", this, m_record.PetEntryId, (uint)m_record.PetEntryId);

						// put back for later (maybe NPCs were not loaded or loaded incorrectly):
						AddPetRecord(activePetRecord);
					}
					else
					{
						SpawnActivePet(activePetRecord);
					}
				}
				else
				{
					// active Pet does not exist in DB
					m_record.PetEntryId = 0;
					m_record.IsPetActive = false;
				}
			}
		}

		void SpawnActivePet(IPetRecord record)
		{
			//AddPostUpdateMessage(() =>
			AddMessage(() =>
			{
				var petSettings = m_record as IActivePetSettings;
				var pet = SpawnPet(record, ref m_position, petSettings.PetDuration);

				pet.CreationSpellId = petSettings.PetSummonSpellId;
				pet.Health = petSettings.PetHealth;
				pet.Power = petSettings.PetPower;
			});
		}

		void AddPetRecord(IPetRecord record)
		{
			if (record is SummonedPetRecord)
			{
				SummonedPetRecords.Add((SummonedPetRecord) record);
			}
			else if (record is PermanentPetRecord)
			{
				StabledPetRecords.Add((PermanentPetRecord)record);
			}
			else
			{
				log.Warn("Unclassified PetRecord: " + record);
			}
		}

		internal void SaveEntourage()
		{
			if (m_activePet != null)
			{
				m_activePet.UpdatePetData(m_record);
				m_activePet.PetRecord.Save();
			}

			CommitPetChanges(m_StabledPetRecords);
			CommitPetChanges(m_SummonedPetRecords);
		}

		private static void CommitPetChanges<T>(IList<T> records)
			where T : IPetRecord
		{
			if (records != null)
			{
				for (var i = 0; i < records.Count; i++)
				{
					var record = records[i];
					if (record.IsDirty)
					{
						record.Save();
					}
				}
			}
		}

		#endregion

		#region Talents
		public void ResetPetTalents()
		{
			if (m_activePet == null) return;

			var price = GodMode ? 0 : m_activePet.Talents.ResetAllPrice;
			if (Money < price) return;

			m_activePet.Talents.ResetAll();
			m_activePet.ResetFreeTalentPoints();
			m_activePet.LastTalentResetTime = DateTime.Now;
			m_activePet.TalentResetPriceTier++;
			Money -= price;
		}

		public bool CanControlExoticPets
		{
			get;
			set;
		}

		public int PetBonusTalentPoints
		{
			get { return m_petBonusTalentPoints; }
			set
			{
				var delta = value - m_petBonusTalentPoints;
				foreach (var pet in StabledPetRecords)
				{
					pet.FreeTalentPoints += delta;
				}
				if (m_activePet != null)
				{
					m_activePet.FreeTalentPoints += delta;
				}
				m_petBonusTalentPoints = value;
			}
		}

		//private void DockPetTalentPoints(int talentBonus)
		//{
		//    if (ActivePet != null)
		//    {
		//        if (ActivePet.FreeTalentPoints < talentBonus)
		//        {
		//            DoPetTalentReset(ActivePet.PetRecord);
		//        }
		//        else
		//        {
		//            ActivePet.FreeTalentPoints -= talentBonus;
		//        }
		//    }

		//    if (m_StabledPetRecords != null)
		//    {
		//        foreach (var pet in m_StabledPetRecords)
		//        {
		//            if (pet.FreeTalentPoints < talentBonus)
		//            {
		//                DoPetTalentReset(pet);
		//            }
		//            else
		//            {
		//                pet.FreeTalentPoints -= talentBonus;
		//            }
		//        }
		//    }
		//}

		public PetTalentType PetTalentType
		{
			get { return PetTalentType.None; }
		}
		#endregion

		#region Records
		public SummonedPetRecord GetOrCreateSummonedPetRecord(NPCEntry entry)
		{
			var record = GetOrCreatePetRecord(entry, SummonedPetRecords);
			record.PetNumber = (uint)PetMgr.PetNumberGenerator.Next();
			return record;
		}

		public T GetOrCreatePetRecord<T>(NPCEntry entry, IList<T> list)
			where T : IPetRecord, new()
		{
			foreach (var record in list)
			{
				if (record.EntryId == entry.NPCId)
				{
					return record;
				}
			}
			return PetMgr.CreateDefaultPetRecord<T>(entry, m_record.EntityLowId);
		}

		#endregion

		#region Minion Events
		protected internal override void OnMinionEnteredRegion(NPC minion)
		{
			base.OnMinionEnteredRegion(minion);
			if (minion.Entry.Type == CreatureType.Totem)
			{
				if (m_totems == null)
				{
					m_totems = new NPC[PetMgr.MaxTotemSlots];
				}

				var index = minion.GetTotemIndex();
				if (m_totems[index] != null)
				{
					m_totems[index].Delete();
				}
				m_totems[index] = minion;
			}
			else if (minion != m_activePet)
			{
				if (m_minions == null)
				{
					m_minions = new NPCCollection();
				}
				m_minions.Add(minion);
			}
		}

		protected internal override void OnMinionLeftRegion(NPC minion)
		{
			base.OnMinionLeftRegion(minion);
			if (minion == m_activePet)
			{
				if (m_activePet.PetRecord != null)
				{
					m_activePet.UpdatePetData(m_record);
					((ActiveRecordBase)m_activePet.PetRecord).SaveLater();
				}
			}
			else if (minion.Entry.Type == CreatureType.Totem)
			{
				if (m_totems != null)
				{
					var index = minion.GetTotemIndex();
					if (m_totems[index] == minion)
					{
						m_totems[index] = null;
					}
				}
			}
			else if (m_minions != null)
			{
				m_minions.Remove(minion);
			}
		}

		/// <summary>
		/// Called when a Pet or 
		/// </summary>
		/// <param name="minion"></param>
		protected internal override void OnMinionDied(NPC minion)
		{
			base.OnMinionDied(minion);
			if (minion == m_activePet)
			{
				IsPetActive = false;
			}
			else if (m_minions != null)
			{
				m_minions.Remove(minion);
			}
		}
		#endregion

		#region GOs
		public void AddOwnedGO(GameObject go)
		{
			if (m_ownedGOs == null)
			{
				m_ownedGOs = new List<GameObject>();
			}
			go.Master = this;
			m_ownedGOs.Add(go);
		}

		public bool OwnsGo(GOEntryId goId)
		{
			if (m_ownedGOs == null)
			{
				return false;
			}
			foreach (var go in m_ownedGOs)
			{
				if (go.Entry.GOId == goId)
				{
					return true;
				}
			}
			return false;
		}

		public GameObject GetOwnedGO(GOEntryId id)
		{
			if (m_ownedGOs != null)
			{
				return m_ownedGOs.Find(go => id == go.Entry.GOId);
			}
			return null;
		}

		public GameObject GetOwnedGO(uint slot)
		{
			if (m_ownedGOs != null)
			{
				foreach (var go in m_ownedGOs)
				{
					if (go.Entry.SummonSlotId == slot)
					{
						return go;
					}
				}
			}
			return null;
		}

		public void RemoveOwnedGO(uint slot)
		{
			if (m_ownedGOs != null)
			{
				foreach (var go in m_ownedGOs)
				{
					if (go.Entry.SummonSlotId == slot)
					{
						go.Delete();
						return;
					}
				}
			}
		}

		public void RemoveOwnedGO(GOEntryId goId)
		{
			if (m_ownedGOs != null)
			{
				foreach (var go in m_ownedGOs)
				{
					if ((GOEntryId)go.EntryId == goId)
					{
						go.Delete();
						return;
					}
				}
			}
		}

		public void OnOwnedGODestroyed(GameObject go)
		{
			if (m_ownedGOs != null)
			{
				m_ownedGOs.Remove(go);
			}
		}
		#endregion
	}
}