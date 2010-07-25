using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Effects;

namespace WCell.RealmServer.Spells
{
	public partial class SpellCast
	{
		/// <summary>
		/// Checks whether the given target resisted the debuff, represented through the given spell
		/// </summary>
		public static CastMissReason CheckDebuffResist(Unit target, Spell spell, int casterLevel, bool hostile)
		{
			var missReason = CastMissReason.None;
			if (hostile && target.CheckDebuffResist(casterLevel, target.GetLeastResistant(spell)))
			{
				missReason = CastMissReason.Resist;
			}
			return missReason;
		}

		private SpellFailedReason PrepAuras()
		{
			// create Auras
			//m_auraApplicationInfos = AuraAppListPool.Obtain();
			m_auraApplicationInfos = new List<AuraApplicationInfo>(4);

			// check stacking
			var casterInfo = CasterObject.CasterInfo;
			SpellEffectHandler lastHandler = null;
			for (var i = 0; i < m_handlers.Length; i++)
			{
				var spellHandler = m_handlers[i];
				if (spellHandler.Effect.IsAuraEffect)
				{
					if (lastHandler != null && lastHandler.Effect.ImplicitTargetA == spellHandler.Effect.ImplicitTargetA)
					{
						continue;
					}
					lastHandler = spellHandler;

					var doubleTarget = false;
					if (spellHandler.Targets != null)
					{
						foreach (var target in spellHandler.Targets)
						{
							if (target is Unit)
							{
								foreach (var info in m_auraApplicationInfos)
								{
									if (info.Target == target)
									{
										doubleTarget = true;
										break;
									}
								}

								if (doubleTarget)
								{
									doubleTarget = false;
									continue;
								}

								var id = m_spell.GetAuraUID(casterInfo, target);
								var failReason = SpellFailedReason.Ok;
								if (((Unit)target).Auras.CheckStackOrOverride(casterInfo, id, m_spell, ref failReason))
								{
									m_auraApplicationInfos.Add(new AuraApplicationInfo((Unit)target));
								}
								else if (failReason != SpellFailedReason.Ok && !IsAoE)
								{
									// spell fails
									// m_auraApplicationInfos.Clear();
									// AuraAppListPool.Recycle(m_auraApplicationInfos);
									return failReason;
								}
							}
						}
					}
				}
			}
			return SpellFailedReason.Ok;
		}

		void CreateAuras(ref List<CastMiss> missedTargets, ref List<IAura> auras, DynamicObject dynObj)
		{
			auras = AuraListPool.Obtain();
			var allowDead = m_spell.PersistsThroughDeath;

			// create AreaAura
			if (m_spell.IsAreaAura)
			{
				if (allowDead || Caster == null || Caster.IsAlive)
				{
					// AreaAura is created at the target location if it is a DynamicObject, else its applied to the caster
					var areaAura = new AreaAura(dynObj as WorldObject ?? Caster, m_spell);
					auras.Add(areaAura);
				}
			}

			// remove missed targets
			for (var i = m_auraApplicationInfos.Count-1; i >= 0; i--)
			{
				var app = m_auraApplicationInfos[i];
				if (!m_targets.Contains(app.Target))
				{
					m_auraApplicationInfos.RemoveAt(i);
				}
			}
			if (m_auraApplicationInfos.Count == 0)
			{
				return;
			}

			// create Aura-Handlers
			for (var i = 0; i < m_handlers.Length; i++)
			{
				var spellHandler = m_handlers[i];
				if (spellHandler is ApplyAuraEffectHandler)
				{
					((ApplyAuraEffectHandler) spellHandler).AddAuraHandlers(m_auraApplicationInfos);
				}
			}
			if (missedTargets == null)
			{
				missedTargets = CastMissListPool.Obtain();
			}

			// apply all new Auras
			for (var i = 0; i < m_auraApplicationInfos.Count; i++)
			{
				var info = m_auraApplicationInfos[i];
				var target = info.Target;

				if (info.Handlers == null || (!allowDead && !target.IsAlive))
				{
					continue;
				}

				// check for immunities and resistances
				CastMissReason missReason;
				var hostile = m_spell.IsHarmfulFor(CasterObject, target);
				var casterInfo = CasterObject.CasterInfo;

				if (!IsPassive && !m_spell.IsPreventionDebuff && 
					(missReason = CheckDebuffResist(target, m_spell, casterInfo.Level, hostile)) != CastMissReason.None)
				{
					missedTargets.Add(new CastMiss(target, missReason));
				}
				else
				{
					var newAura = target.Auras.AddAura(casterInfo, m_spell, info.Handlers, !m_spell.IsPreventionDebuff && !hostile);
					if (newAura != null)
					{
						auras.Add(newAura);

						// check for debuff
						if (!m_spell.IsPreventionDebuff && hostile && target.IsInWorld)
						{
							// force combat mode
							target.IsInCombat = true;
						}
					}
				}
			}

			//m_auraApplicationInfos.Clear();
			//AuraAppListPool.Recycle(m_auraApplicationInfos);
			m_auraApplicationInfos = null;
		}
	}
}