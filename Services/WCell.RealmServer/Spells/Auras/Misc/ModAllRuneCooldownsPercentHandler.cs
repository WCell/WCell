using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Misc
{
    /// <summary>
    /// Modifies all rune cooldowns by the EffectValue in percent
    /// </summary>
    public class ModAllRuneCooldownsPercentHandler : AuraEffectHandler
    {
        private float[] deltas;

        protected override void Apply()
        {
            var chr = Owner as Character;
            if (chr != null && chr.PlayerSpells.Runes != null)
            {
                deltas = chr.PlayerSpells.Runes.ModAllCooldownsPercent(EffectValue);
            }
        }

        protected override void Remove(bool cancelled)
        {
            if (deltas != null)
            {
                // remove the deltas again
                var runes = ((Character)Owner).PlayerSpells.Runes;
                for (RuneType t = 0; t < RuneType.End; t++)
                {
                    runes.ModCooldown(t, -deltas[(int)t]);
                }
            }
        }
    }
}
