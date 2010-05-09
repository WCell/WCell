//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using WCell.Constants.Spells;
//using WCell.RealmServer.AI.Actions.Combat;
//using WCell.RealmServer.AI.Actions.Movement;
//using WCell.RealmServer.AI.Actions.Spells;
//using WCell.RealmServer.Entities;
//using WCell.RealmServer.NPCs;
//using WCell.RealmServer.Spells;

//namespace WCell.RealmServer.AI.Actions
//{
//    public interface IAIActionGenerator
//    {
//        IAIActionCollection Generate(NPC npc);
//    }

//    public class AIActionGeneratorDummy : IAIActionGenerator
//    {
//        public IAIActionCollection Generate(NPC npc)
//        {
//            var collection = new AIActionCreatorCollection();

//            return collection;
//        }
//    }

//    public class DefaultAIActionGenerator : IAIActionGenerator
//    {
//        public IAIActionCollection Generate(NPC npc)
//        {
//            var collection = new AIActionCreatorCollection();

//            // Moving
//            collection.AddCreator(AIActionType.MoveToPoint, (owner) => new AIPointMovementAction(owner));
//            collection.AddCreator(AIActionType.MoveToTarget, (owner) => new AIMoveToTargetAction(owner));

//            collection.AddApproachAndExecuteFactory(AIActionType.InCombatMeleeWhiteDamage,
//                (brain) => new AIMeleeAttackAction(brain),
//                (brain) => {
//                    var owner = brain.Owner;

//                    if (owner.Target == null)
//                        return -1;

//                    float distanceSq = owner.GetDistanceToTargetSq(owner.Target);

//                    float minRangeSq = owner.MainWeapon.MinRange,
//                          maxRangeSq = owner.MainWeapon.MaxRange;

//                    minRangeSq *= minRangeSq;
//                    maxRangeSq *= minRangeSq;
//                    if (distanceSq >= minRangeSq && distanceSq <= maxRangeSq)
//                        return 100;
//                    else if (distanceSq < minRangeSq)
//                        return 50;
//                    else
//                        return 0;
//                });

//            var spells = npc.Spells;
//            var meleeSpellRange = npc.MainWeapon.MaxRange;

//            foreach (Spell spell in spells.SpellsById.Values)
//            {
//                var spellCopy = spell; // using spell instead of spellCopy in lambdas cause unexpected behavior (modifying closures)

//                if (spell.IsDamageSpell)
//                {
//                    if (spell.Range.Max <= meleeSpellRange) // melee dmg spell
//                    {
//                        collection.AddApproachAndExecuteFactory(AIActionType.InCombatMeleeSpellDamage,
//                                                                (brain) => new AISpellCastAction(brain, spellCopy),
//                                                                (brain) => {
//                                                                    var owner = brain.Owner;

//                                                                    if (owner.HasEnoughPowerToCast(spellCopy, owner.Target) &&
//                                                                        !owner.Spells.IsCoolingDown(spellCopy))
//                                                                    {
//                                                                        if (owner.IsInRangeSq(owner.Target,
//                                                                                            spellCopy.Range.Min * spellCopy.Range.Min,
//                                                                                            spellCopy.Range.Max * spellCopy.Range.Max))
//                                                                            return 10;
//                                                                        else
//                                                                            return 1;
//                                                                    }

//                                                                    return -1;
//                                                                });
//                    }

//                    if (spell.IsRanged || spell.Range.Max > meleeSpellRange) // ranged dmg spell
//                    {
//                        collection.AddApproachAndExecuteFactory(AIActionType.InCombatRangedSpellDamage,
//                                                                (brain) => new AISpellCastAction(brain, spellCopy),
//                                                                (brain) => {
//                                                                    var owner = brain.Owner;

//                                                                    if (owner.HasEnoughPowerToCast(spellCopy, owner.Target) &&
//                                                                        !owner.Spells.IsCoolingDown(spellCopy))
//                                                                    {
//                                                                        if (owner.IsInRangeSq(owner.Target,
//                                                                        spellCopy.Range.Min * spellCopy.Range.Min,
//                                                                        spellCopy.Range.Max * spellCopy.Range.Max))
//                                                                            return 10;
//                                                                        else
//                                                                            return 1;
//                                                                    }

//                                                                    return -1;
//                                                                });
//                    }
//                }

//                if (spell.IsHealSpell && !spell.HasHarmfulEffects)
//                {
//                    collection.AddCreator(AIActionType.HealSelf,
//                                          (brain) => new AISpellCastAction(brain, spellCopy),
//                                          (brain) => {
//                                              var owner = brain.Owner;

//                                              if (owner.HasEnoughPowerToCast(spellCopy, owner.Target) &&
//                                                  !owner.Spells.IsCoolingDown(spellCopy))
//                                              {
//                                                  if (owner.IsInRangeSq(owner.Target,
//                                                                      spellCopy.Range.Min * spellCopy.Range.Min,
//                                                                      spellCopy.Range.Max * spellCopy.Range.Max))
//                                                      return 10;
//                                                  else
//                                                      return 0;
//                                              }

//                                              return -1;
//                                          });
//                }
//            }

//            return collection;
//        }
//    }
//}
