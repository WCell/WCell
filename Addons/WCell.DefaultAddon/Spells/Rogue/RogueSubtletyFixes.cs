using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.Constants;
using WCell.RealmServer.Spells.Auras.Handlers;
using WCell.RealmServer.Spells.Effects;

namespace WCell.Addons.Default.Spells.Rogue
{
    public static class RogueSubtletyFixes
    {
        [Initialization(InitializationPass.Second)]
        public static void FixRogue()
        {
            SpellLineId.RogueVanish.Apply(spell =>
			{
				spell.AddTriggerSpellEffect(SpellId.ClassSkillStealth);

                var effect = spell.GetEffectsWhere(eff => (int)eff.TriggerSpellId == 18461).First();
                spell.RemoveEffect(effect);
            });

            SpellLineId.RogueStealth.Apply(spell =>
            {
                var effect = spell.GetEffect(AuraType.ModStealth);
                effect.AuraEffectHandlerCreator = () => new RogueStealthHandler();
            });

            SpellLineId.RogueCloakOfShadows.Apply(spell =>
            {
                var effect = spell.GetEffect(SpellEffectType.TriggerSpell);
                effect.SpellEffectHandlerCreator = (cast, eff) => new CloakOfShadowsHandler(cast, eff);
            });

            SpellLineId.RogueSubtletyPreparation.Apply(Spell =>
            {
                var effect = Spell.GetEffect(SpellEffectType.Dummy);
                effect.SpellEffectHandlerCreator = (cast, eff) => new PreparationHandler(cast, eff);
            });

			SpellHandler.Apply(spell =>
			{
				spell.Effects[0].AuraEffectHandlerCreator =
				() => new MasterOfSubtletyPeriodicHandler();
			}, SpellId.MasterOfSubtlety_2);

        }
    }

    #region CloakOfShadowsHandler
    class CloakOfShadowsHandler : SpellEffectHandler
    {
        public CloakOfShadowsHandler(SpellCast cast, SpellEffect effect) : base(cast, effect)
		{
		}

        public override void Apply()
        {
            var chr = m_cast.CasterChar;

            if(chr != null)
            {
                chr.Auras.RemoveWhere(aura => aura.Spell.HasHarmfulEffects);
            }
        }
    }
    #endregion

    #region PreperationHandler
    class PreparationHandler : SpellEffectHandler
    {
        private SpellLineId[] spellsWithoutGlyph = new[]
        {
            SpellLineId.RogueVanish,
            SpellLineId.RogueEvasion,
            SpellLineId.RogueSprint,
            SpellLineId.RogueAssassinationColdBlood,
            SpellLineId.RogueSubtletyShadowstep
        };

        private SpellLineId[] spellsWithGlyph = new[]
        {
            SpellLineId.RogueCombatBladeFlurry,
            SpellLineId.RogueDismantle,
            SpellLineId.RogueKick
        };

        public PreparationHandler(SpellCast cast, SpellEffect effect) : base(cast, effect)
		{
		}

        protected override void Apply(RealmServer.Entities.WorldObject target)
        {
            var chr = target as Character;
            if(chr != null)
            {
                foreach(var line in spellsWithoutGlyph)
                {
                    line.Apply(spell =>
                    {
                        if(chr.Spells.Contains(spell.Id))
                        { 
                            chr.Spells.ClearCooldown(spell, false);
                        }
                    });
                }
                if (chr.Spells.Contains(SpellId.GlyphOfPreparation) || chr.Spells.Contains(SpellId.GlyphOfPreparation_2))
                {
                    foreach (var line in spellsWithGlyph)
                    {
                        line.Apply(spell =>
                        {
                            if (chr.Spells.Contains(spell.Id))
                            {
                                chr.Spells.ClearCooldown(spell, false);
                            }
                        });
                    }
                }
            }
        }
    }
#endregion

    #region StealthHandler
	class RogueStealthHandler : ModStealthHandler
    {
		protected override void Apply()
		{
			base.Apply();
			var chr = m_aura.Owner as Character;
			if (chr != null)
			{
				//Overkill
				if (chr.Spells.Contains(SpellId.RogueAssassinationOverkill))
				{
					var ovk = chr.Auras[SpellId.ClassSkillOverkill];
					if (ovk != null)
					{
						ovk.Duration = -1; //no duration/last forever
						AuraHandler.SendAuraUpdate(m_aura.Owner, ovk);
					}
					else
						chr.Auras.CreateAndStartAura(m_aura.CasterReference, SpellHandler.Get(SpellId.ClassSkillOverkill), true);
				}
				//Master of Subtlety
				if (chr.Auras[SpellLineId.RogueSubtletyMasterOfSubtlety] != null)
				{
					chr.Auras.Remove(SpellId.MasterOfSubtlety_2);//remove periodic dummy so dmg buff doesn't get removed if casting stealth again
					var masterofsub = chr.Auras[SpellLineId.RogueSubtletyMasterOfSubtlety, true];
					var customspell = SpellHandler.Get(SpellId.MasterOfSubtlety);
					customspell.Effects[0].BasePoints = masterofsub.Spell.Effects[0].BasePoints;
					chr.Auras.CreateAndStartAura(m_aura.CasterReference, customspell, true);
				}
			}
		}

        protected override void Remove(bool cancelled)
        {
			base.Remove(cancelled);
            var chr = m_aura.Owner as Character;
            if(chr != null)
            {
            	chr.Auras.Remove(SpellLineId.RogueVanish);

				//Overkill
				var overkill = chr.Auras[SpellId.ClassSkillOverkill];
				if (overkill != null)
				{
					overkill.Duration = 20000;//20 sec
					AuraHandler.SendAuraUpdate(m_aura.Owner, overkill);
				}
				//Master of Subtlety
				if (chr.Auras[SpellId.MasterOfSubtlety] != null)
				{
					chr.SpellCast.Trigger(SpellId.MasterOfSubtlety_2, chr);//trigger periodic dummy
				}
            }
        }
    }
    #endregion
	#region MasterOfSubtletyPeriodicHandler
	public class MasterOfSubtletyPeriodicHandler : PeriodicDamageHandler
	{
		protected override void Apply()
		{
			if (m_aura.Owner is Character)
			{
				var chr = (Character)m_aura.Owner;
				chr.Auras.Remove(SpellId.MasterOfSubtlety);//remove dmg buff after 6 sec
			}
		}
	}
	#endregion

}