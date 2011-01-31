using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Spells
{
	public class AISpellTargetCollection : SpellTargetCollection
	{
		public static AISpellTargetCollection ObtainAICollection()
		{
			// TODO: Recycle
			return new AISpellTargetCollection();
		}

		
		// TODO: Determine whether targets are valid
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


		#region FindValidAICasterTargets
		/// <summary>
		/// Returns a set of potential targets for this Spell and the given caster, 
		/// using this Spell's AIBehavior as parameters.
		/// </summary>
		WorldObject[] FindValidAICasterTargets()
		{
			var cast = Cast;
			var caster = cast.CasterUnit;
			var spell = cast.Spell;

			// TODO: Use effect radius
			//if (caster.IsInSpellRange(Spell, m_target))
			//{

			//}

			var aiSettings = spell.AISettings;

			// find single target
			WorldObject singleTarget;
			switch (aiSettings.TargetType)
			{
				case AISpellCastTarget.NearestHostilePlayer:
					singleTarget = caster.GetNearestUnit(obj => obj is Character && caster.IsHostileWith(obj));
					break;
				case AISpellCastTarget.RandomAlliedUnit:
					singleTarget = caster.GetNearbyRandomAlliedUnit();
					break;
				case AISpellCastTarget.RandomHostilePlayer:
					singleTarget = caster.GetNearbyRandomHostileCharacter();
					break;
				case AISpellCastTarget.SecondHighestThreatTarget:
					if (!(caster is NPC))
					{
						return null;
					}
					var npc = (NPC)caster;
					singleTarget = npc.ThreatCollection.GetAggressorByThreatRank(1);
					break;
				default:
					singleTarget = null;
					break;
			}

			if (singleTarget != null)
			{
				return new[] { singleTarget };
			}
			return null;
		}
		#endregion
	}
}
