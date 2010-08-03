using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Auras.Handlers;
using WCell.RealmServer.Spells.Effects;

namespace WCell.Addons.Default.Spells.DeathKnight
{
	public class DeathKnightUnholyFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixIt()
		{
			// Unholy Virulence has some incorrect restrictions
			SpellLineId.DeathKnightUnholyVirulence.Apply(spell =>
			{
				// Improves all Spells
				spell.GetEffect(AuraType.ModSpellHitChance).MiscValue = 0;
			});

			FixUnholyFever();

			// Corpse explosion does not explode a corpse
			SpellLineId.DeathKnightUnholyCorpseExplosion.Apply(spell =>
			{
				// TODO: Animation
				var effect1 = spell.Effects[0];
				effect1.TriggerSpellId = SpellId.ClassSkillCorpseExplosion;
				effect1.SpellEffectHandlerCreator = (cast, effct) => new CorpseExplosionHandler(cast, effct);

				spell.Effects[1].SpellEffectHandlerCreator = (cast, effct) => new VoidEffectHandler(cast, effct);
			});

			// needs to override effect value with the value of the effect that triggered it
			SpellHandler.Apply(spell => spell.GetEffect(SpellEffectType.SchoolDamage).OverrideEffectValue = true,
							   SpellId.ClassSkillCorpseExplosion);

			FixUnholyBlight();
		}

		#region Unholy Fever
		private static void FixUnholyFever()
		{
			var cryptFeverRanks = new[]
			{
				SpellId.CryptFeverRank1,
				SpellId.CryptFever,
				SpellId.CryptFever_2,
			};

			// Crypt Fever does not proc correctly
			SpellLineId.DeathKnightUnholyCryptFever.Apply(spell =>
			{
				// proc when a new Aura is applied to a target
				spell.ProcTriggerFlags = ProcTriggerFlags.AuraStarted;

				var effect = spell.GetEffect(AuraType.OverrideClassScripts);
				effect.AuraType = AuraType.ProcTriggerSpell;
				effect.TriggerSpellId = cryptFeverRanks[spell.Rank - 1];
				effect.AuraEffectHandlerCreator = () => new CryptFeverHandler();
				effect.ClearAffectMask();
			});

			// The Crypt fever effect should increase damage taken %
			SpellHandler.Apply(spell =>
			{
				var effect = spell.Effects[0];
				effect.AuraType = AuraType.ModDamageTakenPercent;
			}, cryptFeverRanks);
		}

		/// <summary>
		/// CF may only proc on diseases
		/// </summary>
		internal class CryptFeverHandler : ProcTriggerSpellHandler
		{
			public override bool CanProcBeTriggeredBy(IUnitAction action)
			{
				return action.Spell != null && action.Spell.DispelType == DispelType.Disease;
			}
		}
		#endregion

		#region Unholy Blight
		static void FixUnholyBlight()
		{
			// Unholy Blight needs to proc an Aura when Death Coil is casted
			SpellLineId.DeathKnightUnholyUnholyBlight.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.SpellCast;

				var effect = spell.GetEffect(AuraType.Dummy);
				effect.AuraType = AuraType.ProcTriggerSpell;
				effect.AddToAffectMask(SpellLineId.DeathKnightDeathCoil);
				effect.TriggerSpellId = SpellId.ClassSkillUnholyBlight;
				//effect.AuraEffectHandlerCreator = () => new UnholyBlightHandler();
			});
			SpellHandler.Apply(spell =>
			{
				// add periodic damage
				spell.GetEffect(AuraType.PeriodicDamage).AuraEffectHandlerCreator = () => new UnholyBlightDamageHandler();

				// "preventing any diseases on the victim from being dispelled"
				var dispelImmEffect = spell.AddAuraEffect(AuraType.DispelImmunity);
				dispelImmEffect.MiscValue = (int)DispelType.Disease;
			}, SpellId.ClassSkillUnholyBlight);
		}

		public class UnholyBlightDamageHandler : ParameterizedPeriodicDamageHandler
		{
			protected override void CheckInitialize(SpellCast creatingCast, ObjectReference casterReference, Unit target, ref SpellFailedReason failReason)
			{
				if (creatingCast == null || !(creatingCast.TriggerAction is DamageAction))
				{
					failReason = SpellFailedReason.Error;
					return;
				}

				var daction = ((DamageAction)creatingCast.TriggerAction);

				// "taking $49194s1% of the damage done by the Death Coil over $50536d"
				TotalDamage = daction.ActualDamage;
			}
		}
		#endregion
	}

	#region Corpse Explosion
	public class CorpseExplosionHandler : TriggerSpellEffectHandler
	{
		public const float CorpseSearchRadius = 10;
		public static uint CorpseDisplayId = 11401;

		public CorpseExplosionHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override bool HasOwnTargets
		{
			get { return false; }
		}

		static bool IsValidCorpse(NPC corpse)
		{
			// this check won't let us use the spell on corpses of NPCs that by default have the CorpseDisplayId
			return corpse != null && !corpse.IsAlive && corpse.DisplayId != CorpseDisplayId;
		}

		public override void Apply()
		{
			// find corpse
			var corpse = m_cast.Selected as NPC;
			if (!IsValidCorpse(corpse))
			{
				// find corpse nearby
				var target = m_cast.Selected ?? m_cast.CasterUnit;
				if (target == null)
				{
					return;		// should not happen
				}
				target.IterateEnvironment(CorpseSearchRadius, obj =>
				{
					corpse = obj as NPC;
					return !IsValidCorpse(corpse);
				});
			}

			if (corpse != null)
			{
				corpse.DisplayId = CorpseDisplayId;
				m_cast.Selected = corpse;
				m_cast.TargetLoc = corpse.Position;
				base.Apply();
			}
		}
	}
	#endregion
}
