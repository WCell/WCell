using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Pets;
using WCell.Constants.Updates;
using WCell.RealmServer.AI;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.Items;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.Network;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Pets;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Talents;
using PetNameInvalidReason = WCell.Constants.Pets.PetNameInvalidReason;
using WCell.Constants.Spells;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.Entities
{
	public partial class NPC
	{
		public IPetRecord PetRecord
		{
			get { return m_PetRecord; }
			set { m_PetRecord = value; }
		}

		public PermanentPetRecord PermanentPetRecord
		{
			get { return m_PetRecord as PermanentPetRecord; }
		}

		/// <summary>
		/// Whether this is the active pet of it's master (with an action bar)
		/// </summary>
		public bool IsActivePet
		{
			get { return m_master is Character && ((Character)m_master).ActivePet == this; }
		}

		/// <summary>
		/// Whether this is a Hunter pet.
		/// See http://www.wowwiki.com/Hunter_pet
		/// </summary>
		public bool IsHunterPet
		{
			get { return m_PetRecord is PermanentPetRecord && PetTalentType == PetTalentType.End; }
		}

		#region Names
		/// <summary>
		/// Validates the given name and sends the reason if it was not valid.
		/// </summary>
		/// <param name="chr">The pet's owner.</param>
		/// <param name="name">The proposed name.</param>
		/// <returns>True if the name is kosher.</returns>
		public PetNameInvalidReason TrySetPetName(Character chr, string name)
		{
			if (!PetMgr.InfinitePetRenames && !chr.GodMode)
			{
				if (!PetState.HasFlag(PetState.CanBeRenamed))
				{
					return PetNameInvalidReason.Invalid;
				}
			}

			var response = PetMgr.ValidatePetName(ref name);
			if (response != PetNameInvalidReason.Ok)
			{
				return response;
			}
			Name = name; // also sets the timestamp
			PetState &= ~PetState.CanBeRenamed;
			return response;
		}
		#endregion

		/// <summary>
		/// Makes this the pet of the given owner
		/// </summary>
		internal void MakePet(uint ownerId)
		{
			PetRecord = PetMgr.CreatePermanentPetRecord(Entry, ownerId);
			if (!HasTalents && IsHunterPet)
			{
				m_petTalents = new PetTalentCollection(this);
			}
		}

		/// <summary>
		/// Is called when this Pet became the ActivePet of a Character
		/// </summary>
		internal void OnBecameActivePet()
		{
			OnLevelChanged();
		}

		public bool CanEat(PetFoodType petFoodType)
		{
			return m_entry.Family != null && m_entry.Family.PetFoodMask.HasAnyFlag(petFoodType);
		}

		public int GetHappinessGain(ItemTemplate food)
		{
			if (food == null) return 0;

			// TODO: Replace unnamed with named constants
			var diff = (Level - (int)food.Level);
			if (diff > 0)
			{
				if (diff < 16)
				{
					return PetMgr.MaxFeedPetHappinessGain;
				}
				if (diff < 26)
				{
					return (PetMgr.MaxFeedPetHappinessGain / 2);
				}
				if (diff < 36)
				{
					return (PetMgr.MaxFeedPetHappinessGain / 4);
				}
			}
			else
			{
				if (diff > -16)
				{
					return PetMgr.MaxFeedPetHappinessGain;
				}
			}
			return 0;
		}

		#region Talents
		public PetTalentType PetTalentType
		{
			get { return Entry.Family != null ? Entry.Family.PetTalentType : PetTalentType.End; }
		}

		public DateTime? LastTalentResetTime
		{
			get;
			set;
		}

		public bool HasTalents
		{
			get { return m_petTalents != null; }
		}

		/// <summary>
		/// Collection of all this Pet's Talents
		/// </summary>
		public override TalentCollection Talents
		{
			get { return m_petTalents; }
		}

		public int FreeTalentPoints
		{
			get { return GetByte(UnitFields.BYTES_1, 1); }
			set
			{
				if (m_PetRecord is PermanentPetRecord)
				{
					PermanentPetRecord.FreeTalentPoints = value;
					SetByte(UnitFields.BYTES_1, 1, (byte)value);
					TalentHandler.SendTalentGroupList(m_petTalents);
				}
			}
		}

		public void UpdateFreeTalentPointsSilently(int delta)
		{
			if (m_PetRecord is PermanentPetRecord)
			{
				PermanentPetRecord.FreeTalentPoints = delta;
			}
			SetByte(UnitFields.BYTES_1, 1, (byte)delta);
		}

		public void ResetFreeTalentPoints()
		{
			var points = 0;
			var owner = m_master as Character;
			if (owner != null)
			{
				points += owner.PetBonusTalentPoints;
			}

			points += PetMgr.GetPetTalentPointsByLevel(Level);
			FreeTalentPoints = points;
		}
		#endregion

		#region Loading / Saving
		internal void UpdatePetData(IActivePetSettings settings)
		{
			settings.PetEntryId = Entry.NPCId;
			settings.PetHealth = Health;
			settings.PetPower = Power;
			settings.PetDuration = RemainingDecayDelayMillis;
			settings.PetSummonSpellId = CreationSpellId;

			UpdateTalentSpellRecords();

			m_PetRecord.UpdateRecord(this);
		}

		private void UpdateTalentSpellRecords()
		{
			var spellList = new List<PetTalentSpellRecord>();
			foreach (var spell in NPCSpells)
			{
				var cdTicks = NPCSpells.TicksUntilCooldown(spell);
				var cdTime = DateTime.Now.AddMilliseconds(cdTicks * Region.UpdateDelay);
				var spellRecord = new PetTalentSpellRecord
									{
										SpellId = spell.Id,
										CooldownUntil = cdTime
									};
				spellList.Add(spellRecord);
			}
			// TODO: Implement
			// PetRecord.Spells = spellList;
		}
		#endregion

		#region Pet Scaling and levels
		/// <summary>
		/// Whether this NPC currently may gain levels and experience (usually only true for pets and certain kinds of minions)
		/// </summary>
		public bool MayGainExperience
		{
			get { return IsHunterPet && PetExperience < NextPetLevelExperience; }
		}

		public bool MayGainLevels
		{
			get { return HasPlayerMaster && Level <= MaxLevel; }
		}

		public override int MaxLevel
		{
			get
			{
				if (HasPlayerMaster)
				{
					return m_master.Level;
				}
				return base.MaxLevel;
			}
		}

		internal bool TryLevelUp()
		{
			if (MayGainLevels)
			{
				var level = Level;
				var xp = PetExperience;
				var nextLevelXp = NextPetLevelExperience;
				var leveled = false;

				while (xp >= nextLevelXp && level < MaxLevel)
				{
					++level;
					xp -= nextLevelXp;
					nextLevelXp = XpGenerator.GetPetXPForLevel(level + 1);

					leveled = true;
				}

				if (leveled)
				{
					PetExperience = xp;
					NextPetLevelExperience = nextLevelXp;
					Level = level;
					return true;
				}
			}
			return false;
		}

		protected override void OnLevelChanged()
		{
			// scale size, if necessary
			UpdateSize();

			// add/remove spell ranks
			UpdateSpellRanks();

			if (HasPlayerMaster)
			{
				var level = Level;
				if (level >= PetMgr.MinPetTalentLevel)
				{
					// make sure, pet has talent collection
					if (m_petTalents == null)
					{
						m_petTalents = new PetTalentCollection(this);
					}
				}

				if (m_petTalents != null)
				{
					// update talent points
					var freeTalentPoints = Talents.GetFreeTalentPointsForLevel(level);
					if (freeTalentPoints < 0)
					{
						// Level was reduced: Remove talent points
						if (!((Character)m_master).GodMode)
						{
							Talents.RemoveTalents(-freeTalentPoints);
						}
						freeTalentPoints = 0;
					}
					FreeTalentPoints = freeTalentPoints;
				}

				var levelStatInfo = m_entry.GetPetLevelStatInfo(level);
				if (levelStatInfo != null)
				{
					// update pet stats
					ModPetStatsPerLevel(levelStatInfo);
					m_auras.ReapplyAllAuras();
				}
				m_entry.NotifyLeveledChanged(this);
			}
		}

		internal void ModPetStatsPerLevel(PetLevelStatInfo levelStatInfo)
		{
			BaseHealth = levelStatInfo.Health;
			if (PowerType == PowerType.Mana && levelStatInfo.Mana > 0)
			{
				BasePower = levelStatInfo.Mana;
			}

			for (StatType stat = 0; stat < StatType.End; stat++)
			{
				SetBaseStat(stat, levelStatInfo.BaseStats[(int)stat]);
			}

			this.UpdatePetResistance(DamageSchool.Physical);
			SetInt32(UnitFields.HEALTH, MaxHealth);
		}

		private void UpdateSpellRanks()
		{
			if (m_entry.Spells == null) return;
			var level = Level;
			foreach (var spell in m_entry.Spells.Values)
			{
				if (spell.Level > level)
				{
					// remove spells that have a too high level
					m_spells.Remove(spell);
				}
				else
				{
					// add spells that have a low enough level
					m_spells.AddSpell(spell);
				}
			}
		}

		#endregion

		public override int GetUnmodifiedBaseStatValue(StatType stat)
		{
			if (HasPlayerMaster)
			{
				var levelStatInfo = m_entry.GetPetLevelStatInfo(Level);
				if (levelStatInfo != null)
				{
					return levelStatInfo.BaseStats[(int)stat];
				}
			}
			return base.GetUnmodifiedBaseStatValue(stat);
		}

		#region Behavior
		public void SetPetAttackMode(PetAttackMode mode)
		{
			if (m_PetRecord != null)
			{
				m_PetRecord.AttackMode = mode;
			}

			if (mode == PetAttackMode.Passive)
			{
				m_brain.IsAggressive = false;
				m_brain.DefaultState = BrainState.Follow;
			}
			else
			{
				m_brain.IsAggressive = mode == PetAttackMode.Aggressive;
				m_brain.DefaultState = BrainState.Guard;
			}
			m_brain.EnterDefaultState();
		}

		public void SetPetAction(PetAction action)
		{
			switch (action)
			{
				case PetAction.Abandon:
					if (m_master is Character)
					{
						((Character)m_master).ActivePet = null;
					}
					break;
				case PetAction.Follow:
					HasOwnerPermissionToMove = true;
					break;
				case PetAction.Stay:
					HasOwnerPermissionToMove = false;
					break;
				case PetAction.Attack:
					HasOwnerPermissionToMove = true;
					var target = m_master.Target;
					if (target != null && MayAttack(target))
					{
						// remove all aggressors and make the new target a priority
						m_threatCollection.Clear();
						m_threatCollection[target] = int.MaxValue;
						m_brain.State = BrainState.Combat;
					}
					break;
			}
		}

		/// <summary>
		/// Lets this Pet cast the given spell
		/// </summary>
		public void CastPetSpell(SpellId spellId, WorldObject target)
		{
			var spell = NPCSpells.GetReadySpell(spellId);
			SpellFailedReason err;
			if (spell != null)
			{
				if (spell.HasTargets)
				{
					Target = m_master.Target;
				}

				err = spell.CheckCasterConstraints(this);
				if (err == SpellFailedReason.Ok)
				{
					err = SpellCast.Start(spell, false, target != null ? new[] { target } : null);
				}
			}
			else
			{
				err = SpellFailedReason.NotReady;
			}

			if (err != SpellFailedReason.Ok && m_master is IPacketReceiver)
			{
				PetHandler.SendCastFailed((IPacketReceiver)m_master, spellId, err);
			}
		}
		#endregion

		#region Actions
		public uint[] BuidPetActionBar()
		{
			var bar = new uint[PetConstants.PetActionCount];

			var i = 0;

			bar[i++] = new PetActionEntry
			{
				Action = PetAction.Attack,
				Type = PetActionType.SetAction
			}.Raw;

			bar[i++] = new PetActionEntry
			{
				Action = PetAction.Follow,
				Type = PetActionType.SetAction
			}.Raw;

			bar[i++] = new PetActionEntry
			{
				Action = PetAction.Stay,
				Type = PetActionType.SetAction
			}.Raw;

			var spells = m_spells.GetEnumerator();
			for (byte j = 0; j < PetConstants.PetSpellCount; j++)
			{
				if (!spells.MoveNext())
				{
					bar[i++] = new PetActionEntry
					{
						Type = PetActionType.CastSpell2 + j
					}.Raw;
				}
				else
				{
					var spell = spells.Current;
					var actionEntry = new PetActionEntry();
					actionEntry.SetSpell(spell.SpellId, PetActionType.DefaultSpellSetting);
					bar[i++] = actionEntry.Raw;
				}
			}

			bar[i++] = new PetActionEntry
			{
				AttackMode = PetAttackMode.Aggressive,
				Type = PetActionType.SetMode
			}.Raw;

			bar[i++] = new PetActionEntry
			{
				AttackMode = PetAttackMode.Defensive,
				Type = PetActionType.SetMode
			}.Raw;

			bar[i++] = new PetActionEntry
			{
				AttackMode = PetAttackMode.Passive,
				Type = PetActionType.SetMode
			}.Raw;

			return bar;
		}
		#endregion

		#region Totems
		public uint GetTotemIndex()
		{
			if (CreationSpellId != 0)
			{
				var spell = SpellHandler.Get(CreationSpellId);
				if (spell != null && spell.TotemEffect != null)
				{
					var handler = spell.TotemEffect.SummonEntry.Handler as SpellSummonTotemHandler;
					if (handler != null)
					{
						return handler.Index;
					}
				}
			}
			return 0;
		}
		#endregion

		void DeletePetRecord()
		{
			var record = m_PetRecord;
			RealmServer.Instance.AddMessage(record.Delete);
			m_PetRecord = null;
		}
	}
}