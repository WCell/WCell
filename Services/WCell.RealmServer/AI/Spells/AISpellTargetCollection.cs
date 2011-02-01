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
		IEnumerable<WorldObject> FindValidAICasterTargets()
		{
			var handler = FirstHandler;
			var cast = handler.Cast;
			var caster = cast.CasterUnit;
			var spell = cast.Spell;

			// TODO: Use effect radius
			//if (caster.IsInSpellRange(Spell, m_target))
			//{

			//}

			var aiSettings = spell.AISettings;
			var targetType = aiSettings.TargetType != AISpellCastTargetType.Default
			                 	? aiSettings.TargetType
			                 	: handler.Effect.AISpellCastTargetType;

			// find single target
			WorldObject singleTarget;
			var maxDist = spell.Range.MaxDist;
			switch (targetType)
			{
				// default targets
				//case AISpellCastTargetType.Allied:
					
				//    break;

				// special targets
				case AISpellCastTargetType.NearestHostilePlayer:
					singleTarget = caster.GetNearestUnit(maxDist, obj => obj is Character && caster.IsHostileWith(obj));
					break;
				case AISpellCastTargetType.RandomAlliedUnit:
					singleTarget = caster.GetNearbyRandomAlliedUnit(maxDist);
					break;
				case AISpellCastTargetType.RandomHostilePlayer:
					singleTarget = caster.GetNearbyRandomHostileCharacter(maxDist);
					break;
				case AISpellCastTargetType.SecondHighestThreatTarget:
					if (!(caster is NPC))
					{
						singleTarget = null;
					}
					else
					{
						var npc = (NPC) caster;
						singleTarget = npc.ThreatCollection.GetAggressorByThreatRank(1);
						if (singleTarget != null && !singleTarget.IsInRadius(caster, maxDist))
						{
							singleTarget = null;
						}
					}
					break;
				default:
					singleTarget = null;
					break;
			}

			if (singleTarget != null)
			{
				yield return singleTarget;
			}
		}
		#endregion
	}
}
