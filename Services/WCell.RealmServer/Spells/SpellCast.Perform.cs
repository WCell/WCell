using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells
{
	public partial class SpellCast
	{
		/// <summary>
		/// Returns whether the spell can be casted (true) or if immunity of the target prevents it (false)
		/// </summary>
		public static bool CheckImmune(Unit target, Spell spell, bool hostile)
		{
			if (spell.Mechanic != SpellMechanic.None &&
						hostile == spell.Mechanic.IsNegative() &&
						((spell.Mechanic == SpellMechanic.Invulnerable_2 || spell.Mechanic == SpellMechanic.Invulnerable) &&
						!spell.Attributes.HasFlag(SpellAttributes.UnaffectedByInvulnerability) &&
						(target.IsImmune(SpellMechanic.Invulnerable_2) || target.IsImmune(SpellMechanic.Invulnerable))) ||
						(target.IsImmune(spell.Mechanic) || target.IsImmune(spell.DispelType)))
			{
				return false;
			}
			return true;
		}

		#region InitHandlers
		/// <summary>
		/// Creates the SpellEffectHandlers and collects all initial targets
		/// </summary>
		protected SpellFailedReason InitHandlers()
		{
			var failReason = SpellFailedReason.Ok;

			if (m_targets == null)
			{
				//m_targets = WorldObject.WorldObjectSetPool.Obtain();
				m_targets = new HashSet<WorldObject>();
			}

			//var extraEffects = CasterUnit.Spells.GetExtraEffectsForSpell(m_spell.SpellId);
			//var hasExtraEffects = extraEffects != null;
			var handlers = new SpellEffectHandler[m_spell.EffectHandlerCount];// + (hasExtraEffects ? extraEffects.Count : 0)];
			var h = 0;
			SpellTargetCollection targets = null;
			foreach (var effect in m_spell.Effects)
			{
				if (effect.SpellEffectHandlerCreator == null)
				{
					continue;
				}

				CreateHandler(effect, h, handlers, ref targets, ref failReason);
				if (failReason != SpellFailedReason.Ok)
				{
					return failReason;
				}
				h++;
			}
			//if (hasExtraEffects)
			//{
			//    foreach (var effect in extraEffects)
			//    {
			//        if (effect.SpellEffectHandlerCreator == null)
			//        {
			//            continue;
			//        }

			//        InitHandler(effect, h, handlers, out targets, ref failReason);
			//        if (failReason != SpellFailedReason.Ok)
			//        {
			//            return failReason;
			//        }
			//        h++;
			//    }
			//}

			if (failReason == SpellFailedReason.Ok)
			{
				m_handlers = handlers;

				// initialize handlers
				foreach (var handler in m_handlers)
				{
					handler.Initialize(ref failReason);
					if (failReason != SpellFailedReason.Ok)
					{
						m_handlers = null;
						return failReason;
					}
				}

				// initialize targets
				foreach (var handler in m_handlers)
				{
					if (m_initialTargets != null)
					{
						// initialize forced targets
						for (var j = 0; j < m_initialTargets.Length; j++)
						{
							var target = m_initialTargets[j];
							if (target.IsInContext)
							{
								// must call ValidateTarget anyway
								var err = handler.ValidateTarget(target);
								if (err != SpellFailedReason.Ok)
								{
									LogManager.GetCurrentClassLogger().Warn(
										"{0} tried to cast spell \"{1}\" with forced target {2} which is not valid: {3}",
										CasterObject, Spell, target, err);
									if (!IsAoE)
									{
										m_handlers = null;
										return err;
									}
								}
								else if (handler.m_targets != null)
								{
									handler.m_targets.Add(target);
								}
								m_targets.Add(target);
							}
							else if (target.IsInWorld)
							{
								LogManager.GetCurrentClassLogger().Warn(
									"{0} tried to cast spell \"{1}\" with forced target {2} which is not in context",
									CasterObject, Spell, target);
							}
						}
					}
					else
					{
						// Initialize standard Targets
						if (handler.m_targets != null)
						{
							var handlerTargets = handler.m_targets;

							if (!handlerTargets.IsInitialized)
							{
								// find all targets and initialize them
								if ((failReason = handlerTargets.FindAllTargets()) != SpellFailedReason.Ok)
								{
									return failReason;
								}
							}

							foreach (var target in handlerTargets)
							{
								m_targets.Add(target);
							}
						}
					}
				}
			}

			return failReason;
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
			if (m_initialTargets != null)
			{
				// do we have given targets?
				//targets = SpellTargetCollection.SpellTargetCollectionPool.Obtain();
				if (targets == null)
				{
					targets = new SpellTargetCollection();
				}
			}
			else if (handler.HasOwnTargets)
			{
				targets = null;

				// check if we have same target-types, else collect targets specifically for this Effect
				for (var j = 0; j < h; j++)
				{
					var handler2 = handlers[j];
					if (handler.Effect.TargetsEqual(handler2.Effect))
					{
						targets = handler2.m_targets;
						break;
					}
				}

				if (targets == null)
				{
					//targets = SpellTargetCollection.SpellTargetCollectionPool.Obtain();
					targets = new SpellTargetCollection();
				}
			}

			if (targets != null)
			{
				handler.m_targets = targets;
				targets.m_handlers.Add(handler);
			}
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
			// Make sure that there is an Item for Spells that require an Item target
			if (m_spell.TargetFlags.HasAnyFlag(SpellTargetFlags.Item))
			{
				if (UsedItem == null || !UsedItem.IsInWorld || UsedItem.Owner != CasterObject)
				{
					if (m_passiveCast)
					{
						LogManager.GetCurrentClassLogger().Warn("Trying to trigger Spell without Item selected: " + this);
					}
					return SpellFailedReason.ItemNotFound;
				}
				if (UsedItem.IsEquipped && !UsedItem.Unequip())
				{
					if (m_passiveCast)
					{
						LogManager.GetCurrentClassLogger().Warn("Trying to trigger Spell without Item ready: " + this);
					}
					// make sure, Item is not equipped					
					return SpellFailedReason.ItemNotReady;
				}
			}

			if (!GodMode)
			{
				// check whether skill succeeded
				if (CasterObject is Character &&
					m_spell.Ability != null &&
					m_spell.Ability.GreenValue > 0 &&
					!m_spell.Ability.CheckSuccess(((Character)CasterObject).Skills.GetValue(m_spell.Ability.Skill.Id)))
				{
					return SpellFailedReason.TryAgain;
				}

				// consume reagents (last check, must not fail anymore after this one)
				if (!ConsumeReagents())
				{
					return SpellFailedReason.Reagents;
				}

				if (Selected is ILockable && ((ILockable)Selected).Lock != null)
				{
					if (((ILockable)Selected).Lock.RequiresKneeling && CasterObject is Unit)
					{
						((Unit)CasterObject).StandState = StandState.Kneeling;
					}
				}
			}

			var isAutoShot = IsPlayerCast && m_spell.AttributesExB.HasFlag(SpellAttributesExB.AutoRepeat);

			// check immunities
			if (!IsAoE && Selected is Unit && !m_spell.IsPreventionDebuff)
			{
				var hostile = m_spell.IsHarmfulFor(CasterReference, Selected);
				if (!CheckImmune((Unit)Selected, m_spell, hostile))
				{
					Cancel(SpellFailedReason.Immune);
					return SpellFailedReason.Immune;
				}
			}

			// check aura requirements
			if (m_spell.IsAura)
			{
				if (m_targets.Count == 0 && !IsAoE && !m_spell.IsAreaAura)
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

			// break stealth
			if (CasterObject is Unit && !m_spell.AttributesEx.HasFlag(SpellAttributesEx.RemainStealthed))
			{
				((Unit)CasterObject).Auras.RemoveWhere(aura => aura.Spell.DispelType == DispelType.Stealth);
			}

			// toggle autoshot
			if (CasterUnit != null)
			{
				if (isAutoShot)
				{
					if (CasterUnit.Target == null)
					{
						CasterUnit.Target = Selected as Unit;
						if (CasterUnit.Target == null)
						{
							return SpellFailedReason.BadTargets;
						}
					}

					if (CasterUnit.AutorepeatSpell == m_spell)
					{
						// deactivate
						CasterUnit.AutorepeatSpell = null;
					}
					else
					{
						// activate
						CasterUnit.AutorepeatSpell = m_spell;
						SendCastStart();
					}
					CasterUnit.IsFighting = true;
					CasterUnit.IsInCombat = true;
					return SpellFailedReason.DontReport;
				}

				else if (m_spell.Attributes.HasFlag(SpellAttributes.StopsAutoAttack))
				{
					CasterUnit.AutorepeatSpell = null;
					CasterUnit.IsFighting = false;
				}
			}
			else if (m_spell.IsChanneled)
			{
				// Channel requires CasterUnit
				return SpellFailedReason.CasterAurastate;
			}

			return SpellFailedReason.Ok;
		}

		/// <summary>
		/// Performs the actual Spell
		/// </summary>
		internal SpellFailedReason Perform()
		{
			try
			{
				var spell = m_spell;

				SpellFailedReason failReason;
				if (m_handlers == null)
				{
					// initialze Spell handlers
					failReason = InitHandlers();
					if (failReason != SpellFailedReason.Ok)
					{
						Cancel(failReason);
						return failReason;
					}
				}

				//var sw2 = Stopwatch.StartNew();
				failReason = PrePerform();
				if (failReason != SpellFailedReason.Ok)
				{
					Cancel(failReason);
					return failReason;
				}
				//if (CasterChar != null)
				//{
				//    CasterChar.SendSystemMessage("SpellCast (PrePerform): {0} ms", sw2.ElapsedTicks / 10000d);
				//}

				// check whether impact is delayed
				int delay;
				if (spell.ProjectileSpeed > 0 && m_targets.Count > 0)
				{
					float distance;
					if (TriggerAction != null)
					{
						distance = TriggerAction.Attacker.GetDist(TriggerAction.Victim);
					}
					else if (CasterObject != null)
					{
						var target = m_targets.First();
						//var distance = target.GetDistance(Caster) + 10;
						distance = target.GetDistance(CasterObject);
					}
					else
					{
						distance = 0;
					}
					delay = (int)((distance * 1000) / spell.ProjectileSpeed);
				}
				else
				{
					delay = 0;
				}

				var delayedImpact = delay > Map.UpdateDelay / 1000f; // only delay if its noticable

				SpellFailedReason err;
				if (delayedImpact)
				{
					// delayed impact
					if (CasterObject != null)
					{
						CasterObject.CallDelayed(delay, DoDelayedImpact);
						if (!m_spell.IsChanneled && this == CasterObject.SpellCast)
						{
							// reset SpellCast so it cannot be cancelled anymore
							CasterObject.SpellCast = null;
						}
					}
					else
					{
						Map.CallDelayed(delay, () => DoDelayedImpact(null));
					}
					err = SpellFailedReason.Ok;
				}
				else
				{
					// instant impact
					err = Impact(false);
				}

				if (m_casting)
				{

					var runeMask = UsesRunes ? CasterChar.PlayerSpells.Runes.GetActiveRuneMask() : (byte)0;
					if (CasterUnit != null)
					{
						OnCasted();
					}
					// TODO: Fix this in case the spellcast got cancelled
					CheckHitAndSendSpellGo(false, runeMask);
				}

				if (m_casting)
				{
					if (!delayedImpact && !IsChanneling && m_casting)
					{
						Cleanup(true);
					}
				}
				return err;
			}
			catch (Exception e)
			{
				OnException(e);
				return SpellFailedReason.Error;
			}
		}

		private void DoDelayedImpact(WorldObject obj)
		{
			try
			{
				var caster = CasterObject;
				Impact(true);
				if (caster != null && caster.SpellCast == null && !m_passiveCast)
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
		public SpellFailedReason Impact(bool delayed)
		{
			if (!m_casting)
			{
				return SpellFailedReason.Ok;
			}

			// figure out whether targets are still valid if delayed
			List<CastMiss> missedTargets;
			if (delayed)
			{
				CheckCasterValidity();
				missedTargets = CheckHit(m_spell);
			}
			else
			{
				missedTargets = null;
			}

			// apply effects
			foreach (var handler in m_handlers)
			{
				if (handler.Effect.IsPeriodic || handler.Effect.IsStrikeEffect)
				{
					// weapon ability or handled by Aura or Channel
					continue;
				}

				handler.Apply();
				if (!m_casting)
				{
					// the last handler cancelled the SpellCast
					return SpellFailedReason.DontReport;
				}
			}

			if (CasterObject is Unit && m_spell.IsPhysicalAbility)
			{
				// strike at everyone
				foreach (var target in m_targets)
				{
					if (target is Unit)
					{
						((Unit)CasterObject).Strike(GetWeapon(), (Unit)target, this);
					}
				}
			}

			// open Channel and spawn DynamicObject
			DynamicObject dynObj = null;
			if (m_spell.DOEffect != null)
			{
				dynObj = new DynamicObject(this, m_spell.DOEffect.GetRadius(CasterReference));
			}

			if (!m_casting)
			{
				return SpellFailedReason.Ok;
			}

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
				}

				missedTargets.Clear();
				CastMissListPool.Recycle(missedTargets);
			}

			// open channel
			if (m_spell.IsChanneled && CasterObject != null)
			{
				m_channel = SpellChannel.SpellChannelPool.Obtain();
				m_channel.m_cast = this;

				if (CasterObject is Unit)
				{
					if (dynObj != null)
					{
						CasterUnit.ChannelObject = dynObj;
					}
					else if (Selected != null)
					{
						CasterUnit.ChannelObject = Selected;
						if (Selected is NPC && m_spell.IsTame)
						{
							((NPC)Selected).CurrentTamer = CasterObject as Character;
						}
					}
				}

				var len = m_handlers.Length;
				var channelEffectHandlers = SpellEffectHandlerListPool.Obtain();
				//var channelEffectHandlers = new List<SpellEffectHandler>(6);
				for (var i = 0; i < len; i++)
				{
					var handler = m_handlers[i];
					if (handler.Effect.IsPeriodic)
					{
						channelEffectHandlers.Add(handler);
					}
				}
				m_channel.Open(channelEffectHandlers, auras);
			}

			// start Auras
			if (auras != null)
			{
				for (var i = 0; i < auras.Count; i++)
				{
					var aura = auras[i];
					aura.Start(m_spell.IsChanneled ? m_channel : null, false);
				}

				if (!IsChanneling)
				{
					auras.Clear();
					AuraListPool.Recycle(auras);
				}
			}

			// applying debuffs might cancel other Auras
			if (m_spell.HasHarmfulEffects && !m_spell.IsPreventionDebuff)
			{
				foreach (var target in m_targets)
				{
					if (target is Unit && m_spell.IsHarmfulFor(CasterReference, target))
					{
						((Unit)target).Auras.RemoveByFlag(AuraInterruptFlags.OnHostileSpellInflicted);
					}
				}
			}

			//if (CasterChar != null)
			//{
			//    CasterChar.SendSystemMessage("SpellCast (Impact): {0} ms", sw1.ElapsedTicks / 10000d);
			//}

			// clean it up
			if (delayed && !m_spell.IsChanneled && m_casting)
			{
				Cleanup(true);
			}
			return SpellFailedReason.Ok;
		}
		#endregion

		#region OnCasted
		/// <summary>
		/// Called after a Unit caster casted a spell
		/// </summary>
		protected void OnCasted()
		{
			//var sw1 = Stopwatch.StartNew();
			TargetCount = m_targets.Count;

			var caster = CasterUnit;

			// consume combopoints
			if (m_spell.IsFinishingMove)
			{
				caster.ModComboState(null, 0);
			}

			if (caster.IsAlive)
			{
				// sit while eating/drinking
				if (m_spell.IsFood || m_spell.IsDrink)
				{
					caster.StandState = StandState.Sit;
					// food emote, emote of drinking is handled in Aura.OnApply (each amplitude time)
					if (m_spell.IsFood)
					{
						caster.Emote(EmoteType.SimpleEat);
					}
				}

				if (caster is Character)
				{
					// gain skill
					var chr = (Character)caster;
					if (m_spell.Ability != null && m_spell.Ability.CanGainSkill)
					{
						var skill = chr.Skills[m_spell.Ability.Skill.Id];
						var skillVal = skill.CurrentValue;
						var max = (ushort)skill.ActualMax;
						if (skillVal < max)
						{
							skillVal += (ushort)m_spell.Ability.Gain(skillVal);
							skill.CurrentValue = skillVal <= max ? skillVal : max;
						}
					}

					// Using a combat ability
					if (m_spell.IsPhysicalAbility && m_spell.IsRangedAbility)
					{
						if (m_spell.IsThrow)
						{
							// Each throw reduces Durability by one
							var item = chr.RangedWeapon as Item;
							if (item != null)
							{
								item.Durability--;
							}
						}
						else
						{
							// Firing ranged weapons (still) consumes Ammo
							chr.Inventory.ConsumeAmmo();
						}
					}
				}

				// casting a spell on a combatant also puts the Caster in combat mode
				if (!caster.IsInCombat)
				{
					foreach (var target in m_targets)
					{
						if (target is Unit && ((Unit)target).IsInCombat)
						{
							caster.IsInCombat = true;
							break;
						}
					}
				}

				// casting resets the swing delay
				if (m_spell.HasHarmfulEffects && !m_spell.IsPreventionDebuff)
				{
					if (caster.IsInCombat)
					{
						caster.ResetSwingDelay();
					}
				}
			}

			// Used an item
			if (UsedItem != null)
			{
				UsedItem.OnUse();
			}

			// update AuraState
			if (m_spell.RequiredCasterAuraState == AuraState.DodgeOrBlockOrParry)
			{
				caster.AuraState &= ~AuraStateMask.DodgeOrBlockOrParry;
			}

			// generate new proc event
			if (m_spell.GeneratesProcEventOnCast && CasterUnit != null)
			{
				var target = m_targets.FirstOrDefault() as Unit;
				CasterUnit.Proc(ProcTriggerFlags.SpellCast, target,
								new SimpleUnitAction { Attacker = CasterUnit, Victim = target, IsCritical = false, Spell = m_spell },
								true);
			}

			var hasRunes = UsesRunes;
			if (!GodMode)
			{
				// add cooldown (if not autoshot & not triggered by another spell)
				if (!m_spell.AttributesExB.HasFlag(SpellAttributesExB.AutoRepeat) && TriggerEffect == null)
				{
					caster.Spells.AddCooldown(m_spell, CasterItem);
				}
				if (Client != null)
				{
					if (!m_spell.Attributes.HasFlag(SpellAttributes.StartCooldownAfterEffectFade) &&
						CasterItem != null)
					{
						SpellHandler.SendItemCooldown(Client, m_spell.Id, CasterItem);
					}
				}

				// consume runes
				if (hasRunes)
				{
					((Character)caster).PlayerSpells.Runes.ConsumeRunes(Spell);
				}

				// consume power (might cancel the cast due to dying)
				var powerCost = m_spell.CalcPowerCost(caster,
													  Selected is Unit
														? ((Unit)Selected).GetLeastResistantSchool(m_spell)
														: m_spell.Schools[0]);
				if (m_spell.PowerType != PowerType.Health)
				{
					caster.Power -= powerCost;
				}
				else
				{
					caster.Health -= powerCost;
					if (!m_casting)
					{
						return; // we dead!
					}
				}
			}
			else if (!m_passiveCast && caster is Character)
			{
				// clear cooldowns
				var spells = ((Character)caster).PlayerSpells;
				if (spells != null)
				{
					spells.ClearCooldown(m_spell, true);
				}
			}

			// add runic power
			if (hasRunes)
			{
				caster.Power += m_spell.RuneCostEntry.RunicPowerGain;
			}

			// trigger spells after casting spells (used for Forbearance etc)
			if (m_spell.TargetTriggerSpells != null)
			{
				for (var i = 0; i < m_spell.TargetTriggerSpells.Length; i++)
				{
					var trigSpell = m_spell.TargetTriggerSpells[i];
					Trigger(trigSpell, m_targets.ToArray());
					if (!m_casting)
					{
						return; // should not happen (but might)
					}
				}
			}
			if (m_spell.CasterTriggerSpells != null)
			{
				for (var i = 0; i < m_spell.CasterTriggerSpells.Length; i++)
				{
					var trigSpell = m_spell.CasterTriggerSpells[i];
					Trigger(trigSpell, m_targets.ToArray());
					if (!m_casting)
					{
						return; // should not happen (but might)
					}
				}
			}

			// trigger dynamic post-cast spells, eg Shadow Weaving etc
			caster.Spells.TriggerSpellsFor(this);

			// consumes spell modifiers (if required)
			caster.Auras.OnCasted(this);
			if (!m_casting)
			{
				return; // should not happen (but might)
			}

			// Casted event
			m_spell.NotifyCasted(this);

			//if (CasterChar != null)
			//{
			//    CasterChar.SendSystemMessage("SpellCast (Casted): {0} ms", sw1.ElapsedTicks / 10000d);
			//}
		}

		#endregion
	}
}