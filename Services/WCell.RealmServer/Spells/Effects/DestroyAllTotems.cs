using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
	public class DestroyAllTotemsHandler : SpellEffectHandler
	{
		public DestroyAllTotemsHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override bool HasOwnTargets
		{
			get { return false; }
		}

		public override void Apply()
		{
			var chr = m_cast.CasterObject as Character;
			int spellCost = 0;
			if (chr != null)
			{
				if (chr.Totems != null)
				{
					foreach (var totem in chr.Totems)
					{
						if (totem == null)
						{
							continue;
						}
						var spell = SpellHandler.Get(totem.CreationSpellId);
						if (spell != null)
						{
						    if (spell.SpellPower != null)
						    {
						        spellCost += ((chr.BasePower * spell.SpellPower.PowerCostPercentage)/100) / 4;
						    }
						    totem.Delete();
						}
					}
					chr.Energize(spellCost, chr, Effect);
				}
			}
		}
	}
}