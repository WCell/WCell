using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>
    /// Increases healing done by %
    /// </summary>
    public class ModHealingTakenPctHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
        	var owner = Owner as Character;
			if (owner != null)
			{
				owner.HealingTakenModPct += EffectValue;
			}
        }

        protected override void Remove(bool cancelled)
		{
			var owner = Owner as Character;
			if (owner != null)
			{
				owner.HealingTakenModPct -= EffectValue;
			}
        }
    }
};