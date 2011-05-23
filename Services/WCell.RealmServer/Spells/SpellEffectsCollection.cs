using System.Collections.Generic;
using WCell.Constants.Spells;
using WCell.Util;
using WCell.Util.Variables;

namespace WCell.RealmServer.Spells
{
    public static class SpellEffectsCollection
    {
        private static readonly Dictionary<SpellId, SpellEffect[]> SpellEffectsById = new Dictionary<SpellId, SpellEffect[]>();

        [NotVariable]
        public static List<SpellEffect> SpellEffects = new List<SpellEffect>();

        public static void Add(SpellEffect effect)
        {
            SpellEffects.Add(effect);
            SpellEffect[] spellEffects;
            if(!SpellEffectsById.TryGetValue(effect.SpellId, out spellEffects))
            {
                spellEffects = new SpellEffect[3];
                spellEffects[effect.EffectIndex] = effect;
                SpellEffectsById.Add(effect.SpellId, spellEffects);
            }
            else
            {
                spellEffects[effect.EffectIndex] = effect;
                SpellEffectsById[effect.SpellId] = spellEffects;
            }
        }

        public static SpellEffect[] Get(SpellId spellId)
        {
            SpellEffect[] spellEffects;
            SpellEffectsById.TryGetValue(spellId, out spellEffects);
            return spellEffects;
        }

        public static SpellEffect Get(SpellId spellId, uint effectIndex)
        {
            SpellEffect[] spellEffects;
            SpellEffectsById.TryGetValue(spellId, out spellEffects);
            return spellEffects != null ? spellEffects.Get(effectIndex) : null;
        }
    }
}