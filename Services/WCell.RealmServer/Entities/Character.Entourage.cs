using System;
using System.Collections.Generic;
using System.Linq;
using Castle.ActiveRecord;
using WCell.Constants;
using WCell.Constants.GameObjects;
using WCell.Constants.NPCs;
using WCell.Constants.Pets;
using WCell.Core;
using WCell.RealmServer.AI;
using WCell.RealmServer.Database;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Pets;
using WCell.Util.Graphics;

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

        #region ActivePet

        /// <summary>
        /// Currently active Pet of this Character (the one with the action bar)
        /// </summary>
        public NPC ActivePet
        {
            get { return m_activePet; }
            set
            {
                if (value == m_activePet) return;

                if (m_activePet != null)
                {
                    m_activePet.Delete();
                }

                if (IsPetActive = value != null)
                {
                    value.PetRecord.IsActivePet = true;
                    m_record.PetEntryId = value.Entry.NPCId;

                    m_activePet = value;

                    if (m_activePet.PetRecord.ActionButtons == null)
                    {
                        m_activePet.PetRecord.ActionButtons = m_activePet.BuildPetActionBar();
                    }

                    AddPostUpdateMessage(() =>
                    {
                        if (m_activePet == value && m_activePet.IsInContext)
                        {
                            PetHandler.SendSpells(this, m_activePet, PetAction.Follow);
                            PetHandler.SendPetGUIDs(this);
                            m_activePet.OnBecameActivePet();
                        }
                    });
                }
                else
                {
                    Summon = EntityId.Zero;
                    if (Charm == m_activePet)
                    {
                        Charm = null;
                    }
                    m_record.PetEntryId = 0;
                    PetHandler.SendEmptySpells(this);
                    PetHandler.SendPetGUIDs(this);
                    m_activePet = null;
                }
            }
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
                    m_activePet.RemoveFromMap();
                }
                m_record.IsPetActive = value;
            }
        }

        /// <summary>
        /// Dismisses the current pet
        /// </summary>
        public void DismissActivePet()
        {
            if (m_activePet == null) return;
            if (m_activePet.IsSummoned)
            {
                // delete entirely
                AbandonActivePet();
            }
            else
            {
                // just set inactive
                IsPetActive = false;
            }
        }

        /// <summary>
        /// ActivePet is about to be abandoned
        /// </summary>
        public void AbandonActivePet()
        {
            if (m_activePet.IsInWorld && m_activePet.IsHunterPet && !m_activePet.PetRecord.IsStabled)
            {
                m_activePet.RejectMaster();
                m_activePet.IsDecaying = true;
            }
            else
            {
                m_activePet.Delete();
            }
        }

        /// <summary>
        /// Simply unsets the currently active pet without deleting or abandoning it.
        /// Make sure to take care of the pet when calling this method.
        /// </summary>
        private void UnsetActivePet()
        {
            if (m_activePet != null)
            {
            }
        }

        #endregion ActivePet

        #region Controlling

        public void Possess(int duration, Unit target, bool controllable = true, bool sendPetActionsWithSpells = true)
        {
            if (target == null)
                return;
            if (target is NPC)
            {
                Enslave((NPC)target, duration);
                target.Charmer = this;
                Charm = target;
                target.Brain.State = BrainState.Idle;

                if (sendPetActionsWithSpells)
                    PetHandler.SendSpells(this, (NPC)target, PetAction.Stay);
                else
                    PetHandler.SendVehicleSpells(this, (NPC)target);

                SetMover(target, controllable);
                target.UnitFlags |= UnitFlags.Possessed;
            }
            else if (target is Character)
            {
                PetHandler.SendPlayerPossessedPetSpells(this, (Character)target);
                SetMover(target, controllable);
            }
            Observing = target;
            FarSight = target.EntityId;
        }

        public void UnPossess(Unit target)
        {
            Observing = null;
            SetMover(this, true);
            ResetMover();
            FarSight = EntityId.Zero;
            PetHandler.SendEmptySpells(this);

            Charm = null;

            if (target == null)
                return;

            target.Charmer = null;
            target.UnitFlags &= ~UnitFlags.Possessed;

            if (!(target is NPC))
                return;

            target.Brain.EnterDefaultState();
            ((NPC)target).RemainingDecayDelayMillis = 1;
        }

        #endregion Controlling

        #region Taming and Spawning Pets

        /// <summary>
        /// Amount of stabled pets + active pet (if any)
        /// </summary>
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
                    m_StabledPetRecords = new List<PermanentPetRecord>(PetMgr.MaxStableSlots);
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

            InitializeMinion(pet);
            if (IsPetActive)
            {
                m_Map.AddObject(pet);
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

            minion.MakePet(m_record.EntityLowId);
            m_record.PetCount++;

            InitializeMinion(minion);

            // Set the correct pet level
            if (minion.Level < Level - PetMgr.MaxHunterPetLevelDifference)
            {
                minion.Level = Level - PetMgr.MaxHunterPetLevelDifference;
            }
            else if (minion.Level > Level)
            {
                minion.Level = Level;
            }
        }

        /// <summary>
        /// Is called when this Character gets a new minion or pet or when
        /// he changes his ActivePet to the given one.
        /// </summary>
        private void InitializeMinion(NPC pet)
        {
            Summon = pet.EntityId;
            pet.Summoner = this;
            pet.Creator = EntityId;
            pet.PetRecord.SetupPet(pet);
            pet.SetPetAttackMode(pet.PetRecord.AttackMode);

            ActivePet = pet;

            for (var s = DamageSchool.Physical; s < DamageSchool.Count; s++)
            {
                // update all resistances
                pet.UpdatePetResistance(s);
            }
        }

        #endregion Taming and Spawning Pets

        #region Stabling

        /// <summary>
        /// Amount of purchased stable slots
        /// </summary>
        public int StableSlotCount
        {
            get { return Record.StableSlotCount; }
            set { Record.StableSlotCount = value; }
        }

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
            DeStablePet(stabledPermanentPet);
            return true;
        }

        /// <summary>
        /// Tries to have this Character purchase another Stable Slot.
        /// </summary>
        /// <returns>True if successful.</returns>
        public bool TryBuyStableSlot()
        {
            if (StableSlotCount >= PetMgr.MaxStableSlots)
            {
                return false;
            }

            var price = PetMgr.GetStableSlotPrice(StableSlotCount);
            if (Money < price)
            {
                return false;
            }

            Money -= price;
            StableSlotCount++;
            return true;
        }

        #endregion Stabling

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
                    StabledPetRecords.Add(pet);
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

        private void SpawnActivePet(IPetRecord record)
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

        private void AddPetRecord(IPetRecord record)
        {
            if (record is SummonedPetRecord)
            {
                SummonedPetRecords.Add((SummonedPetRecord)record);
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

        #endregion Serialization

        #region Talents

        public bool CanControlExoticPets
        {
            get;
            set;
        }

        /// <summary>
        /// TODO: This seems awfully unsafe and inconsistent
        /// </summary>
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

        #endregion Talents

        #region Records

        internal SummonedPetRecord GetOrCreateSummonedPetRecord(NPCEntry entry)
        {
            var record = GetOrCreatePetRecord(entry, SummonedPetRecords);
            record.PetNumber = (uint)PetMgr.PetNumberGenerator.Next();
            return record;
        }

        internal T GetOrCreatePetRecord<T>(NPCEntry entry, IList<T> list)
            where T : IPetRecord, new()
        {
            foreach (var record in list)
            {
                if (record.EntryId == entry.NPCId)
                {
                    return record;
                }
            }

            if (typeof(T) == typeof(SummonedPetRecord))
            {
                m_record.PetSummonedCount++;
            }
            else
            {
                m_record.PetCount++;
            }
            return PetMgr.CreateDefaultPetRecord<T>(entry, m_record.EntityLowId);
        }

        #endregion Records

        #region Minion Events

        protected internal override void OnMinionEnteredMap(NPC minion)
        {
            base.OnMinionEnteredMap(minion);
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

        protected internal override void OnMinionLeftMap(NPC minion)
        {
            base.OnMinionLeftMap(minion);
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

        #endregion Minion Events

        public void RemoveSummonedEntourage()
        {
            if (Minions != null)
            {
                foreach (var minion in Minions.Where(minion => minion != null))
                {
                    DeleteMinion(minion);
                }
            }

            if (Totems != null)
            {
                foreach (var totem in Totems.Where(totem => totem != null))
                {
                    DeleteMinion(totem);
                }
            }
        }

        private void DeleteMinion(NPC npc)
        {
            if (npc.Summon != EntityId.Zero)
            {
                var summon = Map.GetObject(npc.Summon);
                if (summon != null)
                    summon.Delete();
                npc.Summon = EntityId.Zero;
            }
            npc.Delete();
        }

        #region GOs

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

        internal void AddOwnedGO(GameObject go)
        {
            if (m_ownedGOs == null)
            {
                m_ownedGOs = new List<GameObject>();
            }
            go.Master = this;
            m_ownedGOs.Add(go);
        }

        internal void OnOwnedGODestroyed(GameObject go)
        {
            if (m_ownedGOs != null)
            {
                m_ownedGOs.Remove(go);
            }
        }

        #endregion GOs

        private void DetatchFromVechicle()
        {
            var seat = VehicleSeat;
            if (seat != null)
            {
                seat.ClearSeat();
            }
        }
    }
}