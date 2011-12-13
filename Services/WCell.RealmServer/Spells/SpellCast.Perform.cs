using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Misc;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Targeting;

namespace WCell.RealmServer.Spells
{
	public partial class SpellCast
	{
		#region PrepareHandlers
		private SpellFailedReason PrepareHandlers()
		{
			var failReason = SpellFailedReason.Ok;
			
			var handlers = CreateHandlers(ref failReason);
			if (failReason != SpellFailedReason.Ok)
				return failReason;

			Handlers = handlers;

			failReason = InitializeHandlers();
			if (failReason != SpellFailedReason.Ok)
				return failReason;

			return InitializeHandlersTargets();
		}

		private SpellEffectHandler[] CreateHandlers(ref SpellFailedReason failReason)
		{
			var handlers = new SpellEffectHandler[Spell.EffectHandlerCount];
			var h = 0;
			SpellTargetCollection targets = null;
			foreach (var effect in Spell.Effects.Where(effect => effect.SpellEffectHandlerCreator != null))
			{
				CreateHandler(effect, h, handlers, ref targets, ref failReason);
				if (failReason != SpellFailedReason.Ok)
				{
					return null;
				}
				h++;
			}

			return handlers;
		}

		private void CreateHandler(SpellEffect effect, int h, SpellEffectHandler[] handlers, ref SpellTargetCollection targets, ref SpellFailedReason failReason)
		{
			var handler = effect.SpellEffectHandlerCreator(this, effect);
			handlers[h] = handler;

			// make sure, we have the right Caster-Type
			handler.CheckCasterType(ref failReason);
			if (failReason != SpellFailedReason.Ok)
			{
				return;
			}

			// find targets and amount SpellTargetCollection if effects have same ImplicitTargetTypes
			if (InitialTargets != null)
			{
				// do we have given targets?
				if (targets == null)
				{
					targets = CreateSpellTargetCollection();
				}
			}
			else if (handler.HasOwnTargets)
			{
				// see if targets are shared between effects
				targets = null;

				for (var j = 0; j < h; j++)
				{
					var handler2 = handlers[j];
					if (handler.Effect.SharesTargetsWith(handler2.Effect, IsAICast))
					{
						// same targets -> share target collection
						targets = handler2.m_targets;
						break;
					}
				}

				if (targets == null)
				{
					targets = CreateSpellTargetCollection();
				}
			}

			if (targets != null)
			{
				handler.m_targets = targets;
				targets.m_handlers.Add(handler);
			}
		}

		private SpellFailedReason InitializeHandlers()
		{
			foreach (var handler in Handlers)
			{
				var failReason = handler.Initialize();
				
				if (failReason != SpellFailedReason.Ok)
				{
					Handlers = null;
					return failReason;
				}
			}

			return SpellFailedReason.Ok;
		}

		private SpellFailedReason InitializeHandlersTargets()
		{
			foreach (var handler in Handlers.Where(handler => handler.Targets != null && !handler.Targets.IsInitialized))
			{
				var failReason = CollectHandlerTargets(handler);
				if (failReason != SpellFailedReason.Ok)
					return failReason;
			}

			return SpellFailedReason.Ok;
		}

		private SpellFailedReason CollectHandlerTargets(SpellEffectHandler handler)
		{
			var failReason = InitialTargets != null ? handler.Targets.AddAll(InitialTargets) : handler.Targets.FindAllTargets();

			if (failReason != SpellFailedReason.Ok)
				return failReason;

			AddHandlerTargetsToTargets(handler);

			return SpellFailedReason.Ok;
		}

		private void AddHandlerTargetsToTargets(SpellEffectHandler handler)
		{
			foreach (var target in handler.Targets)
			{
				Targets.Add(target);
			}
		}

		SpellTargetCollection CreateSpellTargetCollection()
		{
			if (IsAICast)
			{
				return AISpellTargetCollection.ObtainAICollection();
			}
			return SpellTargetCollection.Obtain();
		}
		#endregion

		#region Perform
		private void Perform(int elapsed)
		{
			CheckCasterValidity();
			Perform();
		}

		/// <summary>
		/// Does some sanity checks and adjustments right before perform
		/// </summary>
		protected SpellFailedReason PrePerform()
		{
			if (IsPlayerCast)
			{
				var failReason = PlayerPrePerform();
				if (failReason != SpellFailedReason.Ok)
				{
					return failReason;
				}
			}

			if (CasterUnit == null && Spell.IsChanneled)
			{
				// Channel requires CasterUnit
				return SpellFailedReason.CasterAurastate;
			}

			// check aura stacking and prepare auras
			if (Spell.IsAura)
			{
				if (Targets.Count == 0 && !IsAoE && !Spell.IsAreaAura)
				{
					return SpellFailedReason.NoValidTargets;
				}
				var failReason = PrepAuras();
				if (failReason != SpellFailedReason.Ok)
				{
					Cancel(failReason);
					return failReason;
				}
			}

			if (!GodMode && CasterUnit is Character)
			{
				// check whether skill succeeded
				if (
					Spell.Ability != null &&
					Spell.Ability.GreenValue > 0 &&
					!Spell.Ability.CheckSuccess(((Character)CasterUnit).Skills.GetValue(Spell.Ability.Skill.Id)))
				{
					return SpellFailedReason.TryAgain;
				}

				// consume reagents (last check -> must not fail anymore after this one)
				if (!ConsumeReagents())
				{
					return SpellFailedReason.Reagents;
				}
			}

			if (CasterUnit != null)
			{
				if (SelectedLock != null && SelectedLock.RequiresKneeling)
					CasterUnit.StandState = StandState.Kneeling;

				CancelStealth();
			}

			// revalidate targets for AI, amongst others
			if (IsAICast && !PrePerformAI())
			{
				return SpellFailedReason.NoValidTargets;
			}

			return SpellFailedReason.Ok;
		}

		private SpellFailedReason PlayerPrePerform()
		{
            if (Spell.SpellTargetRestrictions != null)
            {
                if (Spell.SpellTargetRestrictions.TargetFlags.HasAnyFlag(SpellTargetFlags.Item))
                {
                    var failReason = CheckTargetItem();
                    if (failReason != SpellFailedReason.Ok)
                    {
                        return failReason;
                    }
                }
            }

		    if (CastFailsDueToImmune)
			{
				Cancel(SpellFailedReason.Immune);
				return SpellFailedReason.Immune;
			}

			if (Spell.IsAutoRepeating)
			{
				return ToggleAutorepeatingSpell();
			}

			if (Spell.Attributes.HasFlag(SpellAttributes.StopsAutoAttack))
			{
				StopAutoAttack();
			}

			return SpellFailedReason.Ok;
		}

		private SpellFailedReason CheckTargetItem()
		{
			if (!ItemIsSelected)
			{
				LogWarnIfIsPassive("Trying to trigger Spell without Item selected: " + this);

				return SpellFailedReason.ItemNotFound;
			}

			if (!ItemIsReady)
			{
				LogWarnIfIsPassive("Trying to trigger Spell without Item ready: " + this);

				return SpellFailedReason.ItemNotReady;
			}

			return SpellFailedReason.Ok;
		}

	    private bool ItemIsSelected
	    {
	        get { return TargetItem != null && TargetItem.IsInWorld && TargetItem.Owner == CasterObject; } 
	    }


		private void LogWarnIfIsPassive(string message)
		{
			if (IsPassive)
			{
				_log.Warn(message);
			}
		}

	    private bool ItemIsReady
	    {
            get { return !TargetItem.IsEquipped || TargetItem.Unequip(); }
	    }

		private bool CastFailsDueToImmune
		{
			get { return CastCanFailDueToImmune && SelectedUnitIsImmuneToSpell; }
		}

	    private bool CastCanFailDueToImmune
	    {
	        get { return !IsAoE && SelectedTarget is Unit && !Spell.IsPreventionDebuff; }
	    }

	    private bool SelectedUnitIsImmuneToSpell
        {
            get
            {
                var selectedIsHostile = Spell.IsHarmfulFor(CasterReference, SelectedTarget);
                var selectedUnit = (Unit)SelectedTarget;
                return selectedIsHostile && selectedUnit.IsImmuneToSpell(Spell);
            }
        }

		private SpellFailedReason ToggleAutorepeatingSpell()
		{
			if (CasterUnit.Target == null && !(SelectedTarget is Unit))
			{
				return SpellFailedReason.BadTargets;
			}

			CasterUnit.IsFighting = true;
			if (CasterUnit.AutorepeatSpell == Spell)
			{
				DeactivateAutorepeatingSpell();
			}
			else
			{
				ActivateAutorepeatingSpell();
			}

			return SpellFailedReason.DontReport;
		}

		private void ActivateAutorepeatingSpell()
		{
			CasterUnit.AutorepeatSpell = Spell;
			SendCastStart();
		}

		private void DeactivateAutorepeatingSpell()
		{
			CasterUnit.AutorepeatSpell = null;
		}

		private void StopAutoAttack()
		{
			DeactivateAutorepeatingSpell();
			CasterUnit.IsFighting = false;
		}

		private void CancelStealth()
		{
			if (Spell.AttributesEx.HasFlag(SpellAttributesEx.RemainStealthed))
				return;

			CasterUnit.Auras.RemoveWhere(aura => aura.Spell.SpellCategories.DispelType == DispelType.Stealth);
		}

		private LockEntry SelectedLock
		{
			get
			{
				var lockable = SelectedTarget as ILockable;
				return lockable != null ? lockable.Lock : null;
			}
		}

		/// <summary>
		/// Performs the actual Spell
		/// </summary>
		internal SpellFailedReason Perform()
		{
			try
			{
				SpellFailedReason failReason;
				if (Handlers == null)
				{
					failReason = PrepareHandlers();
					if (failReason != SpellFailedReason.Ok)
					{
						Cancel(failReason);
						return failReason;
					}
				}

				failReason = PrePerform();
				if (failReason != SpellFailedReason.Ok)
				{
					Cancel(failReason);
					return failReason;
				}

				List<MissedTarget> missedTargets = CheckHit();
				RemoveFromHandlerTargets(missedTargets);

				SendSpellGo(missedTargets);

				// check whether impact is delayed
				int delay = CalculateImpactDelay();
				var delayedImpactIsNoticable = delay > Map.UpdateDelay;

				if (delayedImpactIsNoticable)
				{
					DoDelayedImpact(delay);
					failReason = SpellFailedReason.Ok;
				}
				else
				{
					// instant impact
					failReason = Impact();
				}

				if (IsCasting && CasterUnit != null)
				{
					OnUnitCasted();
				}

				if (IsCasting && !delayedImpactIsNoticable && !IsChanneling)
				{
					Cleanup();
				}
				return failReason;
			}
			catch (Exception e)
			{
				OnException(e);
				return SpellFailedReason.Error;
			}
		}

        /// <summary>
        /// Calculates the delay until a spell impacts its target in milliseconds
        /// </summary>
        /// <returns>delay in ms</returns>
		private int CalculateImpactDelay()
		{
			if (Spell.ProjectileSpeed <= 0 || Targets.Count == 0)
			{
				return 0;
			}

			float distance;
			if (TriggerAction != null)
			{
				distance = TriggerAction.Attacker.GetDist(TriggerAction.Victim);
			}
			else if (CasterObject != null)
			{
				var target = Targets.First();
				distance = target.GetDistance(CasterObject);
			}
			else
			{
				return 0;
			}

            //projectile speed is distance per second
			return (int)( (distance / Spell.ProjectileSpeed) * 1000);
		}

		private void DoDelayedImpact(int delay)
		{
			if (CasterObject != null)
			{
				CasterObject.CallDelayed(delay, DelayedImpact);
				if (!Spell.IsChanneled && this == CasterObject.SpellCast)
				{
					// reset SpellCast so it cannot be cancelled anymore
					CasterObject.SpellCast = null;
				}
			}
			else
			{
				Map.CallDelayed(delay, () => DelayedImpact(null));
			}
		}

		private void DelayedImpact(WorldObject obj)
		{
			CheckCasterValidity();
			foreach (var target in Targets.Where(target => !target.IsInWorld))
			{
				Remove(target);
			}
			try
			{
				Impact();

				// clean it up
				if (!Spell.IsChanneled && IsCasting)
				{
					Cleanup();
				}

				var caster = CasterObject;
				if (caster != null && caster.SpellCast == null && !IsPassive)
				{
					// recycle spell cast
					// TODO: Improve spellcast recycling
					caster.SpellCast = this;
				}
			}
			catch (Exception e)
			{
				OnException(e);
			}
		}

		#endregion

		#region Impact
		/// <summary>
		/// Validates targets and applies all SpellEffects
		/// </summary>
		public SpellFailedReason Impact()
		{
			if (!IsCasting)
			{
				return SpellFailedReason.Ok;
			}

			// apply effects
			foreach (var handler in Handlers)
			{
				if (handler.Effect.IsPeriodic || handler.Effect.IsStrikeEffect)
				{
					// weapon ability or handled by Aura or Channel
					continue;
				}

				handler.Apply();
				if (!IsCasting)
				{
					// the last handler cancelled the SpellCast
					return SpellFailedReason.DontReport;
				}
			}

			if (CasterObject is Unit && Spell.IsPhysicalAbility)
			{
				// strike at everyone
				foreach (var target in UnitTargets)
				{
					ProcHitFlags hitFlags = CasterUnit.Strike(GetWeapon(), target, this);
					m_hitInfoByTarget[target] = hitFlags;
				}
			}

			// open Channel and spawn DynamicObject
			DynamicObject dynObj = null;
			if (Spell.DOEffect != null)
			{
				dynObj = new DynamicObject(this, Spell.DOEffect.GetRadius(CasterReference));
			}

			if (!IsCasting)
			{
				return SpellFailedReason.Ok;
			}

			List<MissedTarget> missedTargets = null;
			// create auras
			List<IAura> auras = null;
			if (m_auraApplicationInfos != null)
			{
				CreateAuras(ref missedTargets, ref auras, dynObj);
			}

			// check for missed targets
			if (missedTargets != null)
			{
				if (missedTargets.Count > 0)
				{
					// TODO: Flash message ontop of missed heads when impact is delayed
					CombatLogHandler.SendSpellMiss(this, true, missedTargets);
					missedTargets.Clear();
				}

				CastMissListPool.Recycle(missedTargets);
			}

			// open channel
			if (Spell.IsChanneled && CasterObject != null)
			{
				Channel = SpellChannel.SpellChannelPool.Obtain();
				Channel.m_cast = this;

				if (CasterObject is Unit)
				{
					if (dynObj != null)
					{
						CasterUnit.ChannelObject = dynObj;
					}
					else if (SelectedTarget != null)
					{
						CasterUnit.ChannelObject = SelectedTarget;
						if (SelectedTarget is NPC && Spell.IsTame)
						{
							((NPC)SelectedTarget).CurrentTamer = CasterObject as Character;
						}
					}
				}

				var len = Handlers.Length;
				var channelEffectHandlers = SpellEffectHandlerListPool.Obtain();
				//var channelEffectHandlers = new List<SpellEffectHandler>(6);
				for (var i = 0; i < len; i++)
				{
					var handler = Handlers[i];
					if (handler.Effect.IsPeriodic)
					{
						channelEffectHandlers.Add(handler);
					}
				}
				Channel.Open(channelEffectHandlers, auras);
			}

			// start Auras
			if (auras != null)
			{
				for (var i = 0; i < auras.Count; i++)
				{
					var aura = auras[i];
					aura.Start(Spell.IsChanneled ? Channel : null, false);
				}

				if (!IsChanneling)
				{
					auras.Clear();
					AuraListPool.Recycle(auras);
					auras = null;
				}
			}

			// applying debuffs might cancel other Auras
			if (Spell.HasHarmfulEffects && !Spell.IsPreventionDebuff)
			{
				foreach (var target in Targets)
				{
					if (target is Unit && Spell.IsHarmfulFor(CasterReference, target))
					{
						((Unit)target).Auras.RemoveByFlag(AuraInterruptFlags.OnHostileSpellInflicted);
					}
				}
			}

			//if (CasterChar != null)
			//{
			//    CasterChar.SendSystemMessage("SpellCast (Impact): {0} ms", sw1.ElapsedTicks / 10000d);
			//}

			return SpellFailedReason.Ok;
		}
		#endregion

		#region OnUnitCasted
		protected void OnUnitCasted()
		{
			OnAliveUnitCasted();

			OnTargetItemUsed();

			UpdateAuraState();

			if (!GodMode)
			{
				OnNonGodModeSpellCasted();

				if (!IsCasting)
				{
					return; // we dead!
				}
			}
			else if (!IsPassive && CasterUnit is Character)
			{
				ClearCooldowns();
			}

			AddRunicPower();

			TriggerSpellsAfterCastingSpells();
			if (!IsCasting)
			{
				return; // should not happen (but might)
			}

			TriggerDynamicPostCastSpells();
			if (!IsCasting)
			{
				return; // should not happen (but might)
			}

			ConsumeCombopoints();

			ConsumeSpellModifiers();
			if (!IsCasting)
			{
				return; // should not happen (but might)
			}

			if (IsAICast)
			{
				OnAICasted();																					// can execute arbitrary code
				if (!IsCasting)
				{
					return; // should not happen (but might)
				}
			}

			// Casted event
			Spell.NotifyCasted(this);
			if (CasterUnit is Character)
			{
				CasterChar.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.CastSpell, Spell.Id);
			}

			TriggerProcOnCasted();

			m_hitInfoByTarget.Clear();
		}

		private void OnAliveUnitCasted()
		{
			if (!CasterUnit.IsAlive)
				return;

			if (CasterUnit is Character)
			{
				OnAliveCharacterCasted();
			}

			PutCasterInCombatModeAfterCastOnCombatant();

			ResetSwingDelay();
		}

		private void OnAliveCharacterCasted()
		{
			SitWhileConsuming();

			GainSkill();

			if (UsedCombatAbility)
			{
				OnCharacterCombatAbilityUsed();
			}

			CheckForQuestProgress();
		}

		private void SitWhileConsuming()
		{
			if (!Spell.IsFood && !Spell.IsDrink)
				return;

			CasterChar.StandState = StandState.Sit;

			// food emote, emote of drinking is handled in Aura.OnApply (each amplitude time)
			if (Spell.IsFood)
				CasterChar.Emote(EmoteType.SimpleEat);
		}

		private void GainSkill()
		{
			if (Spell.Ability != null && Spell.Ability.CanGainSkill)
			{
				var skill = CasterChar.Skills[Spell.Ability.Skill.Id];
				var skillVal = skill.CurrentValue;
				var max = (ushort)skill.ActualMax;
				if (skillVal < max)
				{
					skillVal += (ushort)Spell.Ability.Gain(skillVal);
					skill.CurrentValue = skillVal <= max ? skillVal : max;
				}
			}
		}

		private bool UsedCombatAbility
		{
			get { return Spell.IsPhysicalAbility && Spell.IsRangedAbility; }
		}

		private void OnCharacterCombatAbilityUsed()
		{
			if (Spell.IsThrow)
			{
				ReduceItemDurability();
			}
			else
			{
				CasterChar.Inventory.ConsumeAmmo();
			}
		}

		private void ReduceItemDurability()
		{
			var item = CasterChar.RangedWeapon as Item;
			if (item != null)
				item.Durability--;
		}

		private void CheckForQuestProgress()
		{
			CasterChar.QuestLog.OnSpellCast(this); // can potentially execute arbitrary code
		}

		private void PutCasterInCombatModeAfterCastOnCombatant()
		{
			if (CasterUnit.IsInCombat)
				return;

			var targetsInCombat = UnitTargets.Where(target => target.IsInCombat).Count();
			if (targetsInCombat > 0)
				CasterUnit.IsInCombat = true;
		}

		private void ResetSwingDelay()
		{
			if (Spell.HasHarmfulEffects && !Spell.IsPreventionDebuff && CasterUnit.IsInCombat)
			{
				CasterUnit.ResetSwingDelay();
			}
		}

		private void OnTargetItemUsed()
		{
			if (TargetItem != null)
			{
				CasterChar.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.UseItem, Spell.Id);
				TargetItem.OnUse(); // can execute arbitrary code
			}
		}

		private void ConsumeSpellModifiers()
		{
			CasterUnit.Auras.OnCasted(this);
		}

		private void TriggerDynamicPostCastSpells()
		{
			CasterUnit.Spells.TriggerSpellsFor(this); // can execute arbitrary code
		}

		private void ConsumeCombopoints()
		{
			if (Spell.IsFinishingMove)
				CasterUnit.ModComboState(null, 0);
		}

		private void TriggerSpellsAfterCastingSpells()
		{
			TriggerTargetTriggerSpells();
			if (!IsCasting)
				return;

			TriggerCasterTriggerSpells();
		}

		private void TriggerCasterTriggerSpells()
		{
			if (Spell.CasterTriggerSpells == null)
				return;

			foreach (var triggerSpell in Spell.CasterTriggerSpells)
			{
				Trigger(triggerSpell, Targets.ToArray()); // can execute arbitrary code
				if (!IsCasting)
					return;
			}
		}

		private void TriggerTargetTriggerSpells()
		{
			if (Spell.TargetTriggerSpells == null)
				return;

			foreach (var triggerSpell in Spell.TargetTriggerSpells)
			{
				Trigger(triggerSpell, Targets.ToArray()); // can execute arbitrary code
				if (!IsCasting)
					return;
			}
		}

		private void AddRunicPower()
		{
			if (UsesRunes)
				CasterUnit.Power += Spell.RuneCostEntry.RunicPowerGain;
		}

		private void OnNonGodModeSpellCasted()
		{
			AddCooldown();

			ConsumeRunes();

			ConsumePower(); // (might cancel the cast due to dying)
		}

		private void ClearCooldowns()
		{
			var spells = CasterChar.PlayerSpells;
			if (spells != null)
				spells.ClearCooldown(Spell);
		}

		private void ConsumePower()
		{
			var powerCost = Spell.CalcPowerCost(CasterUnit,
				SelectedTarget is Unit
					? ((Unit)SelectedTarget).GetLeastResistantSchool(Spell)
					: Spell.Schools[0]);
			if (Spell.PowerType != PowerType.Health)
				CasterUnit.Power -= powerCost;
			else
				CasterUnit.Health -= powerCost;
		}

		private void ConsumeRunes()
		{
			if (UsesRunes)
				CasterChar.PlayerSpells.Runes.ConsumeRunes(Spell);
		}

		private void AddCooldown()
		{
			if (!Spell.IsAutoRepeating && TriggerEffect == null)
				CasterUnit.Spells.AddCooldown(Spell, CasterItem);
			if (Client != null)
			{
				if (!Spell.Attributes.HasFlag(SpellAttributes.StartCooldownAfterEffectFade) &&
					CasterItem != null)
					SpellHandler.SendItemCooldown(Client, Spell.Id, CasterItem);
			}
		}

		private void UpdateAuraState()
		{
			if (Spell.SpellAuraRestrictions != null && Spell.SpellAuraRestrictions.RequiredCasterAuraState == AuraState.DodgeOrBlockOrParry)
				CasterUnit.AuraState &= ~AuraStateMask.DodgeOrBlockOrParry;
		}

		#region Procs
		void TriggerProcOnCasted()
		{
			// Set the flags for caster and target based on the spell
			ProcTriggerFlags casterProcFlags = ProcTriggerFlags.None;
			ProcTriggerFlags targetProcFlags = ProcTriggerFlags.None;

			switch (Spell.SpellCategories.DefenseType)
			{
				case DamageType.None:
				{
					if (Spell.IsBeneficial)
					{
						casterProcFlags |= ProcTriggerFlags.DoneBeneficialSpell;
						targetProcFlags |= ProcTriggerFlags.ReceivedBeneficialSpell;
					}
					else if (Spell.IsHarmful)
					{
						casterProcFlags |= ProcTriggerFlags.DoneHarmfulSpell;
						targetProcFlags |= ProcTriggerFlags.ReceivedHarmfulSpell;
					}
					break;
				}

				case DamageType.Magic:
				{
					if (Spell.IsBeneficial)
					{
						casterProcFlags |= ProcTriggerFlags.DoneBeneficialMagicSpell;
						targetProcFlags |= ProcTriggerFlags.ReceivedBeneficialMagicSpell;
					}
					else if (Spell.IsHarmful)
					{
						casterProcFlags |= ProcTriggerFlags.DoneHarmfulMagicSpell;
						targetProcFlags |= ProcTriggerFlags.ReceivedHarmfulMagicSpell;
					}
					break;
				}

				case DamageType.Melee:
				{
					casterProcFlags |= ProcTriggerFlags.DoneMeleeSpell;
					targetProcFlags |= ProcTriggerFlags.ReceivedMeleeSpell;
					break;
				}

				case DamageType.Ranged:
				{
					if (Spell.IsAutoRepeating)
					{
						casterProcFlags |= ProcTriggerFlags.DoneRangedAutoAttack;
						targetProcFlags |= ProcTriggerFlags.ReceivedRangedAutoAttack;
					}
					else
					{
						casterProcFlags |= ProcTriggerFlags.DoneRangedSpell;
						targetProcFlags |= ProcTriggerFlags.ReceivedRangedSpell;
					}
					break;
				}
			}

			ProcHitFlags casterHitFlags = TriggerProcOnTargets(targetProcFlags);

			TriggerProcOnCaster(casterProcFlags, casterHitFlags);
		}

		/// <summary>
		/// Triggers proc on all targets of SpellCast
		/// </summary>
		/// <param name="flags">What happened to targets ie. ProcTriggerFlags.ReceivedHarmfulSpell</param>
		/// <returns>Combination of hit result on all targets.</returns>
		private ProcHitFlags TriggerProcOnTargets(ProcTriggerFlags flags)
		{
			ProcHitFlags hitFlagsCombination = ProcHitFlags.None;

			foreach (var hitInfo in m_hitInfoByTarget)
			{
				Unit target = hitInfo.Key;
				ProcHitFlags targetHitFlags = hitInfo.Value;

				hitFlagsCombination |= targetHitFlags;

				var action = new SimpleUnitAction
				{
					Attacker = CasterUnit,
					Spell = Spell,
					Victim = target,
					IsCritical = targetHitFlags.HasAnyFlag(ProcHitFlags.CriticalHit)
				};

				target.Proc(flags, CasterUnit, action, true, targetHitFlags);
			}

			return hitFlagsCombination;
		}

		/// <summary>
		/// Trigger proc on the caster of the spell.
		/// </summary>
		/// <param name="flags">What spell caster casted ie. ProcTriggerFlags.DoneHarmfulSpell</param>
		/// <param name="hitFlags">Hit result of the spell</param>
		private void TriggerProcOnCaster(ProcTriggerFlags flags, ProcHitFlags hitFlags)
		{
			var casterAction = new SimpleUnitAction
			{
				Attacker = CasterUnit,
				Spell = Spell,
				Victim = m_hitInfoByTarget.Count > 0 ? m_hitInfoByTarget.First().Key : null,
				IsCritical = hitFlags.HasAnyFlag(ProcHitFlags.CriticalHit)
			};

			var triggerer = UnitTargets.FirstOrDefault();

			CasterUnit.Proc(flags, triggerer, casterAction, true, hitFlags);
		}
		#endregion
		#endregion
	}
}