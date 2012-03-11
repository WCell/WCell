using WCell.Constants;

namespace WCell.RealmServer.Spells.Auras.Mod
{
    /// <summary>
    /// TODO: Reapply when AP changes
    /// </summary>
    public class ModSpellPowerByAPPctHandler : AuraEffectHandler
    {
        private int[] values;

        protected override void Apply()
        {
            var owner = Owner;

            values = new int[m_spellEffect.MiscBitSet.Length];
            for (var i = 0; i < m_spellEffect.MiscBitSet.Length; i++)
            {
                var school = m_spellEffect.MiscBitSet[i];
                var sp = owner.GetDamageDoneMod((DamageSchool)school);
                var val = (sp * EffectValue + 50) / 100;
                values[i] = val;
                owner.AddDamageDoneModSilently((DamageSchool)school, val);
            }
        }

        protected override void Remove(bool cancelled)
        {
            var owner = Owner;

            for (var i = 0; i < m_spellEffect.MiscBitSet.Length; i++)
            {
                var school = m_spellEffect.MiscBitSet[i];
                owner.RemoveDamageDoneModSilently((DamageSchool)school, values[i]);
            }
        }
    }
}