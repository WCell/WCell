using NLog;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spells.Targeting
{

	/// <summary>
	/// Contains default spell targeting Adders.
	/// For more info, refer to: http://wiki.wcell.org/index.php?title=API:Spells#Targeting
	/// </summary>
	public static class DefaultTargetAdders
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		#region Caster, Summon, Pets
		public static void AddSelf(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			var self = targets.Cast.CasterObject;
			if (self == null)
			{
				// invalid trigger proc
				log.Warn("Invalid SpellCast tried to target self, but no Caster given: {0}", targets.Cast);
				failReason = SpellFailedReason.Error;
				return;
			}

			if ((failReason = targets.ValidateTargetForHandlers(self)) == SpellFailedReason.Ok)
			{
				targets.Add(self);
			}
		}

		/// <summary>
		/// Your current summon or self
		/// </summary>
		public static void AddSummon(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			// if (Handler.SpellCast.Caster.Summon == null)
			targets.AddSelf(filter, ref failReason);
		}

		public static void AddPet(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			var caster = targets.Cast.CasterObject;
			if (!(caster is Character))
			{
				log.Warn("Non-Player {0} tried to cast Pet - spell {1}", caster, targets.Cast.Spell);
				failReason = SpellFailedReason.TargetNotPlayer;
				return;
			}

			var pet = ((Character)caster).ActivePet;
			if (pet == null)
			{
				failReason = SpellFailedReason.NoPet;
				return;
			}


			targets.Add(pet);
		}

		//public static void AddOwnPet(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		//{
		//    var chr = targets.Cast.Caster as Character;
		//    if (chr != null)
		//    {
		//        var pet = chr.ActivePet;
		//        if (pet != null)
		//        {
		//            foreach (var handler in targets.m_handlers)
		//            {
		//                if ((failReason = handler.CheckValidTarget(pet)) != SpellFailedReason.Ok)
		//                {
		//                    return;
		//                }
		//            }
		//            targets.Add(pet);
		//        }
		//        else
		//        {
		//            failReason = SpellFailedReason.BadTargets;
		//        }
		//    }
		//    else
		//    {
		//        log.Warn("NPC \"{0}\" used Spell with Pet-Target.", targets.Cast.Caster);
		//        failReason = SpellFailedReason.BadTargets;
		//    }
		//}
		#endregion

		#region Selection
		/// <summary>
		/// Adds the object or unit that has been chosen by a player when the spell was casted
		/// </summary>
		/// <param name="targets"></param>
		public static void AddSelection(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			AddSelection(targets, filter, ref  failReason, false);
		}

		/// <summary>
		/// Adds the selected targets and chain-targets (if any)
		/// </summary>
		public static void AddSelection(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason, bool harmful)
		{
			var cast = targets.Cast;
			if (cast == null)
				return;

			var caster = cast.CasterObject as Unit;
			var selected = cast.Selected;
			if (selected == null)
			{
				if (caster == null)
				{
					log.Warn("Invalid SpellCast, tried to add Selection but nothing selected and no Caster present: {0}", targets.Cast);
					failReason = SpellFailedReason.Error;
					return;
				}
				selected = caster.Target;
				if (selected == null)
				{
					failReason = SpellFailedReason.BadTargets;
					return;
				}
			}

			var effect = targets.FirstHandler.Effect;
			var spell = effect.Spell;
			if (selected != caster && caster != null)
			{
				if (!caster.IsInMaxRange(spell, selected))
				{
					failReason = SpellFailedReason.OutOfRange;
				}
				else if (caster.IsPlayer && !selected.IsInFrontOf(caster))
				{
					failReason = SpellFailedReason.UnitNotInfront;
				}
			}

			if (failReason == SpellFailedReason.Ok)
			{
				// standard checks
				failReason = targets.ValidateTarget(selected, filter);

				if (failReason == SpellFailedReason.Ok)
				{
					// add target and look for more if we have a chain effect
					targets.Add(selected);
					var chainCount = effect.ChainTargets;
					if (caster != null)
					{
						chainCount = caster.Auras.GetModifiedInt(SpellModifierType.ChainTargets, spell, chainCount);
					}
					if (chainCount > 1 && selected is Unit)
					{
						targets.FindChain((Unit)selected, filter, true, chainCount);
					}
				}
			}
		}
		#endregion

		#region AddChannelObject
		public static void AddChannelObject(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			var caster = targets.Cast.CasterUnit;
			if (caster != null)
			{
				if (caster.ChannelObject != null)
				{
					if ((failReason = targets.ValidateTarget(caster.ChannelObject, filter)) == SpellFailedReason.Ok)
					{
						targets.Add(caster.ChannelObject);
					}
				}
				else
				{
					failReason = SpellFailedReason.BadTargets;
				}
			}
		}
		#endregion

		#region AoE
		/// <summary>
		/// Adds targets around the caster
		/// </summary>
		public static void AddAreaSource(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			AddAreaSource(targets, filter, ref  failReason, targets.FirstHandler.GetRadius());
		}

		public static void AddAreaSource(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason, float radius)
		{
			targets.AddTargetsInArea(targets.Cast.SourceLoc, filter, radius);
		}

		/// <summary>
		/// Adds targets around the target area
		/// </summary>
		public static void AddAreaDest(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			AddAreaDest(targets, filter, ref  failReason, targets.FirstHandler.GetRadius());
		}

		public static void AddAreaDest(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason, float radius)
		{
			targets.AddTargetsInArea(targets.Cast.TargetLoc, filter, radius);
		}

		public static void AddChain(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			AddSelection(targets, filter, ref  failReason, false);
		}

		public static void AddTargetsInArea(this SpellTargetCollection targets, Vector3 pos, TargetFilter targetFilter, float radius)
		{
			var handler = targets.FirstHandler;
			var spell = handler.Effect.Spell;
			var cast = handler.Cast;

			int limit;
			if (spell.MaxTargetEffect != null)
			{
				limit = spell.MaxTargetEffect.CalcEffectValue(cast.CasterReference);
			}
			else
			{
				//if IsAllied (used by group/raid spell targeting) it's save to asume the limit is the raid max size (40 players) since some spells have wrong dbc values
				if (targetFilter == DefaultTargetFilters.IsAllied)
				{
					limit = 40;
				}
				else
					limit = (int)spell.MaxTargets;
			}

			if (limit < 1)
			{
				limit = int.MaxValue;
			}

			cast.Map.IterateObjects(pos, radius > 0 ? radius : 5, cast.Phase,
				obj =>
				{
					// AoE spells only make sense on Unit targets (at least there is no known spell that does anything else)
					if (obj is Unit && targets.ValidateTarget(obj, targetFilter) == SpellFailedReason.Ok)
					{
						return targets.AddOrReplace(obj, limit);
					}
					return true;
				});
		}
		#endregion

		#region Party
		public static void AddAllParty(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			var cast = targets.Cast;
			if (cast.CasterChar != null)
			{
				// For Characters: Add the whole party
				if (targets.Cast.CasterChar.Group != null)
				{
					foreach (var member in cast.CasterChar.Group)
					{
						var chr = member.Character;
						if (chr != null)
						{
							targets.Add(chr);
						}
					}
				}
				else
				{
					failReason = SpellFailedReason.TargetNotInParty;
				}
			}
			else
			{
				var radius = targets.FirstHandler.GetRadius();
				if (radius == 0)
				{
					// For NPCs: Add all friendly minions around (radius 30 if no radius is set?)
					radius = 30;
				}
				targets.AddAreaSource(cast.Spell.HasHarmfulEffects ? (TargetFilter)DefaultTargetFilters.IsFriendly : DefaultTargetFilters.IsHostile, ref  failReason, radius);
			}
		}
		#endregion

		#region Items & Objects
		/// <summary>
		/// Used for Lock picking and opening (with or without keys)
		/// </summary>
		public static void AddItemOrObject(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			if (targets.Cast.TargetItem == null && !(targets.Cast.Selected is GameObject))
			{
				failReason = SpellFailedReason.BadTargets;
			}
		}

		public static void AddObject(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			if (!(targets.Cast.Selected is GameObject))
			{
				failReason = SpellFailedReason.BadTargets;
			}
			else
			{
				targets.Add(targets.Cast.Selected);
			}
		}
		#endregion

		/// <summary>
		/// 
		/// </summary>
		public static void AddSecondHighestThreatTarget(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			var caster = targets.Cast.CasterUnit as NPC;
			if (caster == null)
			{
				failReason = SpellFailedReason.NoValidTargets;
				return;
			}

			var nearest = caster.ThreatCollection.GetAggressorByThreatRank(2);

			if (nearest != null)
			{
				targets.Add(nearest);
			}
			else
			{
				failReason = SpellFailedReason.NoValidTargets;
			}
		}
	}
}
