using System.Collections.Generic;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.AI.Spells
{
	public class AISpellTargetCollection : SpellTargetCollection
	{
		public static AISpellTargetCollection ObtainAICollection()
		{
			// TODO: Recycle
			return new AISpellTargetCollection();
		}

		
		// TODO: Determine whether targets are "good" targets

		/// <summary>
		/// Whether the unit should cast the given spell
		/// </summary>
		//private bool ShouldCast(Spell spell)
		//{
		//    var caster = CasterUnit;

		//    if (spell.IsAura)
		//    {
		//        if (spell.CasterIsTarget)
		//        {
		//            if (caster.Auras.Contains(new AuraIndexId(spell.AuraUID, true)))
		//            {
		//                // caster already has Aura
		//                return false;
		//            }
		//        }
		//        else if (spell.HasTargets && !spell.IsAreaSpell)
		//        {
		//            if (target.Auras.Contains(spell))
		//            {
		//                // target already has Aura
		//                return false;
		//            }
		//        }
		//    }
		//    return true;
		//}

	}
}
