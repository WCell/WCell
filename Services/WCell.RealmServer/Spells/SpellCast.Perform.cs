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
						!spell.Attributes.Has(SpellAttributes.UnaffectedByInvulnerability) &&
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
			SpellTargetCollection targets;
			var failReason = SpellFailedReason.Ok;

			if (m_targets == null)
			{
				//m_targets = WorldObject.WorldObjectSetPool.Obtain();
				m_targets = new HashSet<WorldObject>();
			}

			var handlers = new SpellEffectHandler[m_spell.EffectHandlerCount];
			var h = 0;
			for (var i = 0; i < m_spell.Effects.Length; i++)
			{
				// make sure, we have the right Caster-Type
				var effect = m_spell.Effects[i];
				if (effect.SpellEffectHandlerCreator == null)
				{
					continue;
				}

				var handler = effect.SpellEffectHandlerCreator(this, effect);
				handlers[h++] = handler;

				handler.CheckCasterType(ref failReason);
				if (failReason != SpellFailedReason.Ok)
				{
					return failReason;
				}

				handler.Initialize(ref failReason);
				if (failReason != SpellFailedReason.Ok)
				{
					return failReason;
				}

				// find targets and amount SpellTargetCollection if effects have same ImplicitTargetTypes
				targets = null;
				if (m_initialTargets != null)
				{
					// do we have given targets?
					//targets = SpellTargetCollection.SpellTargetCollectionPool.Obtain();
					targets = new SpellTargetCollection();
					for (var j = 0; j < m_initialTargets.Length; j++)
					{
						var target = m_initialTargets[j];
						if (target.IsInWorld)
						{
							var err = handler.CheckValidTarget(target);
							if (err != SpellFailedReason.Ok)
							{
								if (!IsAoE)
								{
									return err;
								}
							}
							else
							{
								targets.Add(target);
							}
						}
					}
				}
				else if (handler.HasOwnTargets)
				{
					// check if we have same target-types, else collect targets specifically for this Effect
					for (var j = 0; j < i; j++)
					{
						var handler2 = handlers[j];
						if (handler.Effect.TargetsEqual(handler2.Effect))
						{
							targets = handler2.Targets;
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
					handler.Targets = targets;
					targets.m_handlers.Add(handler);
				}
			}


			// Initialize Targets
			for (var i = 0; i < handlers.Length; i++)
			{
				var handler = handlers[i];
				if (handler.Targets != null)
				{
					var handlerTargets = handler.Targets;
					// find all targets and init
					if (m_initialTargets == null || m_initialTargets.Length == 0)
					{
						// only search if we don't have initial targets
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
			if (failReason == SpellFailedReason.Ok)
			{
				m_handlers = handlers;
			}
			return failReason;
		}
		#endregion

		#region Perform
		private void Perform(float elapsed)
		{
			if (!Caster.IsInWorld)
			{
				Cleanup(true);
			}
			else
			{
				Perform();
			}
		}

		/// <summary>
		/// Does some last checks right before the performing the Cast.
		/// </summary>
		/// <returns></returns>
		protected SpellFailedReason PrePerform()
		{
			// Make sure that there is an Item for Spells that require an Item target
			if (m_spell.TargetFlags.Has(SpellTargetFlags.Item))
			{
				if (UsedItem == null || !UsedItem.IsInWorld || UsedItem.Owner != Caster)
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
				if (Caster is Character &&
					m_spell.Ability != null &&
					m_spell.Ability.GreenValue > 0 &&
					!m_spell.Ability.CheckSuccess(((Character)Caster).Skills.GetValue(m_spell.Ability.Skill.Id)))
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
					if (((ILockable)Selected).Lock.RequiresKneeling && Caster is Unit)
					{
						((Unit)Caster).StandState = StandState.Kneeling;
					}
				}

				if (IsInstant && !m_spell.IsWeaponAbility)
				{
					SendCastStart();
				}
			}

			if (!IsAoE && Selected is Unit && !m_spell.IsPreventionDebuff)
			{
				var hostile = m_spell.IsHarmfulFor(Caster, Selected);
				if (!CheckImmune((Unit)Selected, m_spell, hostile))
				{
					Cancel(SpellFailedReason.Immune);
					return SpellFailedReason.Immune;
				}
			}

			if (m_spell.IsAura)
			{
				// check aura requirements
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
			if (Caster is Unit && !m_spell.AttributesEx.Has(SpellAttributesEx.RemainStealthed))
			{
				((Unit)Caster).Auras.RemoveWhere(aura => aura.Spell.DispelType == DispelType.Stealth);
			}

			// toggle autoshot
			if (IsPlayerCast && m_spell.AttributesExB.Has(SpellAttributesExB.AutoRepeat))
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

			if (m_spell.Attributes.Has(SpellAttributes.StopsAutoAttack))
			{
				// deactivate
				CasterUnit.AutorepeatSpell = null;
				CasterUnit.IsFighting = false;
			}
			return SpellFailedReason.Ok;
		}

		/// <summary>
		/// Performs the actual Spell
		/// </summary>
		public SpellFailedReason Perform()
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
					var target = m_targets.First();
					//var distance = target.GetDistance(Caster) + 10;
					var distance = target.GetDistance(Caster);
					delay = (int)((distance * 1000) / spell.ProjectileSpeed);
				}
				else
				{
					delay = 0;
				}

				var delayedImpact = delay > Caster.Region.UpdateDelay / 1000f; // only delay if its noticable

				SpellFailedReason err;
				if (m_spell.IsWeaponAbility && !m_spell.IsRangedAbility && Caster is Unit)
				{
					// will be triggered during the next strike
					CasterUnit.m_pendingCombatAbility = this;

					// reset SpellCast so it cannot be cancelled anymore
					CasterUnit.SpellCast = null;
					if (!m_spell.IsOnNextStrike)
					{
						// strike instantly
						CasterUnit.Strike(GetWeapon());
					}
					err = SpellFailedReason.Ok;
				}
				else
				{
					CheckHitAndSendSpellGo(!delayedImpact);
					if (delayedImpact)
					{
						// delayed impact
						Caster.CallDelayed(delay, caster =>
						{
							try
							{
								Impact(true);
							}
							catch (Exception e)
							{
								OnException(e);
							}
						});
						if (!m_spell.IsChanneled && this == Caster.SpellCast)
						{
							// reset SpellCast so it cannot be cancelled anymore
							Caster.SpellCast = null;
						}
						err = SpellFailedReason.Ok;
					}
					else
					{
						// instant impact
						err = Impact(false);
					}
				}

				if (m_casting && !spell.IsWeaponAbility)
				{
					// weapon abilities will call this after execution
					if (Caster is Unit)
					{
						OnCasted();
					}

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
		#endregion

		#region Impact
		/// <summary>
		/// Validates targets and applies all SpellEffects
		/// </summary>
		public SpellFailedReason Impact(bool delayed)
		{
			if (!m_casting || !Caster.IsInWorld)
			{
				return SpellFailedReason.Ok;
			}

			// figure out whether targets are still valid if delayed
			List<CastMiss> missedTargets;
			if (delayed)
			{
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

			// open Channel and spawn DynamicObject
			DynamicObject dynObj = null;
			if (m_spell.DOEffect != null)
			{
				dynObj = new DynamicObject(this, m_spell.DOEffect.GetRadius(Caster));
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
					CombatLogHandler.SendSpellMiss(m_spell.SpellId, Caster, true, missedTargets);
				}

				missedTargets.Clear();
				CastMissListPool.Recycle(missedTargets);
			}

			// channeling
			if (m_spell.IsChanneled)
			{
				m_channel = SpellChannel.SpellChannelPool.Obtain();
				m_channel.m_cast = this;

				if (Caster is Unit)
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
							((NPC)Selected).CurrentTamer = Caster as Character;
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
					if (target is Unit && m_spell.IsHarmfulFor(Caster, target))
					{
						((Unit)target).Auras.RemoveByFlag(AuraInterruptFlags.OnHostileSpellInflicted);
					}
				}
			}

			// check for weapon abilities
			if (m_spell.IsWeaponAbility && !IsChanneling)
			{
				if (Caster is Unit)
				{
					if (m_spell.IsRangedAbility)
					{
						// reset SpellCast so it cannot be cancelled anymore
						CasterUnit.SpellCast = null;
						CasterUnit.m_pendingCombatAbility = this;
						CasterUnit.Strike(GetWeapon());
					}
					
					if (!IsChanneling)
					{
						OnCasted();
					}
				}
			}

			//if (CasterChar != null)
			//{
			//    CasterChar.SendSystemMessage("SpellCast (Impact): {0} ms", sw1.ElapsedTicks / 10000d);
			//}

			// clean it up
			if ((delayed || m_spell.IsWeaponAbility) && (!m_spell.IsChanneled) && m_casting)
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

			var caster = (Unit)Caster;

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

				// gain skill
				if (Caster is Character)
				{
					var chr = (Character)Caster;
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

			// Using a combat ability
			if (m_spell.IsWeaponAbility && m_spell.IsRangedAbility && Caster is Character)
			{
				if (m_spell.IsThrow)
				{
					// Each throw reduces Durability by one
					var item = ((Character)Caster).RangedWeapon as Item;
					if (item != null)
					{
						item.Durability--;
					}
				}
				else
				{
					// Firing ranged weapons (still) consumes Ammo
					((Character)Caster).Inventory.ConsumeAmmo();
				}
			}

			if (!GodMode)
			{
				// add cooldown (if not autoshot)
				if (!m_spell.AttributesExB.Has(SpellAttributesExB.AutoRepeat) && !m_spell.IsTriggeredSpell)
				{
					caster.Spells.AddCooldown(m_spell, CasterItem);
				}

				if (Client != null)
				{
					if ((m_spell.Attributes & SpellAttributes.StartCooldownAfterEffectFade) == 0 &&
						CasterItem != null)
					{
						SpellHandler.SendItemCooldown(Client, m_spell.Id, CasterItem);
					}
				}

				// consume power (might cancel the cast due to dying)
				var powerCost = m_spell.CalcPowerCost(caster,
													  Selected is Unit
														? ((Unit)Selected).GetLeastResistant(m_spell)
														: m_spell.Schools[0],
														m_spell,
														m_spell.PowerType);
				if (m_spell.PowerType != PowerType.Health)
				{
					caster.Power -= powerCost;
				}
				else
				{
					caster.Health -= powerCost;
					if (!m_casting)
					{
						return;		// should not happen (but might)
					}
				}
			}
			// here SpellCast might already be cleaned up
			else if (!m_passiveCast && GodMode && Caster is Character)
			{
				// clear cooldowns
				var spells = ((Character)Caster).PlayerSpells;
				if (spells != null)
				{
					spells.ClearCooldown(m_spell);
				}
			}

			// trigger fixed post-cast spells, such as Forbearance etc
			if (m_spell.TargetTriggerSpells != null)
			{
				for (var i = 0; i < m_spell.TargetTriggerSpells.Length; i++)
				{
					var trigSpell = m_spell.TargetTriggerSpells[i];
					Trigger(trigSpell, m_targets.ToArray());
					if (!m_casting)
					{
						return;		// should not happen (but might)
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
						return;		// should not happen (but might)
					}
				}
			}

			// Custom procs to be added to caster or targets
			if (m_spell.TargetProcHandlers != null)
			{
				for (var i = 0; i < m_spell.TargetProcHandlers.Count; i++)
				{
					var proc = m_spell.TargetProcHandlers[i];
					foreach (var target in m_targets)
					{
						if (target is Unit)
						{
							((Unit)target).AddProcHandler(new ProcHandler((Unit)target, proc));
						}
					}
				}
			}
			if (m_spell.CasterProcHandlers != null && Caster is Unit)
			{
				for (var i = 0; i < m_spell.CasterProcHandlers.Count; i++)
				{
					var proc = m_spell.CasterProcHandlers[i];
					((Unit)Caster).AddProcHandler(new ProcHandler((Unit)Caster, proc));
				}
			}

			// trigger dynamic post-cast spells, eg Shadow Weave etc, and consumes spell modifiers (if required)
			if (Caster is Character)
			{
				((Character)Caster).PlayerSpells.OnCasted(this);
				if (!m_casting)
				{
					return;		// should not happen (but might)
				}
			}

			// Casted event
			var evt = Casted;
			if (evt != null)
			{
				evt(this);
			}

			//if (CasterChar != null)
			//{
			//    CasterChar.SendSystemMessage("SpellCast (Casted): {0} ms", sw1.ElapsedTicks / 10000d);
			//}
		}
		#endregion
	}
}
