using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Pets;
using WCell.Constants.Updates;
using WCell.RealmServer.AI;
using WCell.RealmServer.AI.Actions.Combat;
using WCell.RealmServer.Database;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.Items;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.Network;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Pets;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Talents;
using WCell.Util;
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
			get { return m_PetRecord is PermanentPetRecord; }
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
			get { return Entry.Family != null ? Entry.Family.PetTalentType : PetTalentType.None; }
		}

		public DateTime? LastTalentResetTime
		{
			get;
			set;
		}

		public SpecProfile SpecProfile
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
		public TalentCollection Talents
		{
			get
			{
				if (m_petTalents == null)
				{
					m_petTalents = new TalentCollection(this);
				}
				return m_petTalents;
			}
		}

		public int FreeTalentPoints
		{
			get { return GetByte(UnitFields.BYTES_1, 1); }
			set
			{
				if (m_PetRecord is PermanentPetRecord)
				{
					PermanentPetRecord.FreeTalentPoints = value;
				}
				SetByte(UnitFields.BYTES_1, 1, (byte)value);
				TalentHandler.SendTalentGroupList(this);
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

		public int TalentResetPriceTier
		{
			get { return m_PetRecord is PermanentPetRecord ? ((PermanentPetRecord)m_PetRecord).TalentResetPriceTier : 0; }
			set
			{
				if (m_PetRecord is PermanentPetRecord)
				{
					if (value < 0)
					{
						value = 0;
					}
					if (value > (PetMgr.PetTalentResetPriceTiers.Length - 1))
					{
						value = (PetMgr.PetTalentResetPriceTiers.Length - 1);
					}
					((PermanentPetRecord)m_PetRecord).TalentResetPriceTier = value;
				}
			}
		}

		public int GetTalentResetPrice()
		{
			if (m_master != null && m_master is Character)
			{
				if (((Character)m_master).GodMode)
				{
					return 0;
				}
			}

			var tiers = PetMgr.PetTalentResetPriceTiers;
			var lastPriceTier = TalentResetPriceTier;
			var lastResetTime = LastTalentResetTime;

			if (lastResetTime == null)
			{
				return tiers[0];
			}

			var timeLapse = DateTime.Now - lastResetTime.Value;
			var numDiscounts = timeLapse.Hours / 2;
			var newPriceTier = lastPriceTier - numDiscounts;

			if (newPriceTier < 0)
			{
				return tiers[0];
			}

			if (newPriceTier > (tiers.Length - 1))
			{
				return tiers[tiers.Length - 1];
			}
			return tiers[newPriceTier];
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
			SetScale();
			PetExperience = 0;
			if (HasPlayerMaster)
			{
				var level = Level;
				if (HasTalents)
				{
					var freeTalentPoints = Talents.GetFreePetTalentPoints(level);
					if (freeTalentPoints < 0)
					{
						// need to remove talent points
						if (!((Character)m_master).GodMode)
						{
							// remove the extra talents
							Talents.RemoveTalents(-freeTalentPoints);
						}
						freeTalentPoints = 0;
					}
					FreeTalentPoints = freeTalentPoints;
				}

				var levelStatInfo = m_entry.GetPetLevelStatInfo(level);
				if (levelStatInfo != null)
				{
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

			// update spell ranks
			var done = true;
			do
			{
				foreach (var spell in m_spells)
				{
					if (spell.Talent == null && spell.NextRank != null && spell.NextRank.Level <= Level)
					{
						done = false;
						m_spells.Remove(spell);
						m_spells.AddSpell(spell.NextRank);
						break; // start new iteration because iterator was invalidated
					}
				}
			} while (!done);
			SetInt32(UnitFields.HEALTH, MaxHealth);
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

			if (err != SpellFailedReason.Ok)
			{
				if (m_master.IsPlayer)
				{
					PetHandler.SendCastFailed(m_master as IPacketReceiver, spellId, err);
				}
			}
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