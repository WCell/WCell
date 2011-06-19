using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Util;

namespace WCell.RealmServer.Spells.Targeting
{
	public static class DefaultTargetDefinitions
	{
		public static readonly TargetDefinition[] DefaultTargetHandlers = new TargetDefinition[(int)Utility.GetMaxEnum<ImplicitSpellTargetType>() + 1];

		/// <summary>
		/// Returns the handler and filter for the given target type
		/// </summary>
		public static TargetDefinition GetTargetDefinition(ImplicitSpellTargetType target)
		{
			return DefaultTargetHandlers[(int)target];
		}

		public static TargetFilter GetTargetFilter(ImplicitSpellTargetType target)
		{
			var def = DefaultTargetHandlers[(int)target];
			return def != null ? def.Filter : null;
		}

		#region Init
		static DefaultTargetDefinitions()
		{
			InitTargetHandlers();
		}

		private static void InitTargetHandlers()
		{
			DefaultTargetHandlers[(int)ImplicitSpellTargetType.AllAroundLocation] = new TargetDefinition(
					DefaultTargetAdders.AddAreaDest,	// Is this right?
					null);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.AllEnemiesAroundCaster] = new TargetDefinition(
					DefaultTargetAdders.AddAreaSource,
					DefaultTargetFilters.IsHostile);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.AllEnemiesInArea] = new TargetDefinition(
					DefaultTargetAdders.AddAreaDest,
					DefaultTargetFilters.IsHostile);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.AllEnemiesInAreaInstant] = new TargetDefinition(
					DefaultTargetAdders.AddAreaDest,
					DefaultTargetFilters.IsHostile);

			// ImplicitTargetType.AllFriendlyInAura

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.AllParty] = new TargetDefinition(
					DefaultTargetAdders.AddAllParty,
					null);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.AllPartyAroundCaster] = new TargetDefinition(
					DefaultTargetAdders.AddAreaSource,
					DefaultTargetFilters.IsAllied);

			// Seems to be bogus: Often used together with AllEnemiesAroundCaster
			DefaultTargetHandlers[(int)ImplicitSpellTargetType.AllPartyInArea] = new TargetDefinition(
					DefaultTargetAdders.AddAreaDest,
					DefaultTargetFilters.IsAllied);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.AllPartyInAreaChanneled] = new TargetDefinition(
					DefaultTargetAdders.AddAreaDest,
					DefaultTargetFilters.IsAllied);

			// Odd: Mostly in combination with LocationToSummon and TeleportLocation
			// The only spell that has this with a negative effect is: Goblin Mortar (Id: 13238, Target: Default)
			DefaultTargetHandlers[(int)ImplicitSpellTargetType.AllTargetableAroundLocationInRadiusOverTime] = new TargetDefinition(
					DefaultTargetAdders.AddAreaSource,
					DefaultTargetFilters.IsFriendly);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.AreaEffectPartyAndClass] = new TargetDefinition(
					DefaultTargetAdders.AddAreaSource,
					DefaultTargetFilters.IsSamePartyAndClass);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.BehindTargetLocation] = new TargetDefinition(
				DefaultTargetAdders.AddSelection,
				null);

			// ImplicitTargetType.CaliriEggs

			// any kind of chain effect
			DefaultTargetHandlers[(int)ImplicitSpellTargetType.Chain] = new TargetDefinition(
					DefaultTargetAdders.AddChain,
					null);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.CurrentSelection] = new TargetDefinition(
					DefaultTargetAdders.AddSelection,
					null);
			/*
			Spell: ClassSkill Dispel Magic, Rank1 (Id: 527, Target: Default)
	School: Holy
	DispelType: 1
	Attributes: Flag0x10000
	AttributesExB: Flag0x80000

			 * Spell: Charm (Possess) (Id: 530, Target: Default)
	School: Arcane
	Attributes: Flag0x10000
	AttributesExC: Flag0x10000000
			 * 
			 Spell: Periodic Mana Burn (Id: 812, Target: Default)
	Attributes: Flag0x100, Flag0x800, Flag0x800000
	AttributesExB: Flag0x4
			 */

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.Duel] = new TargetDefinition(
					DefaultTargetAdders.AddSelection,
					DefaultTargetFilters.IsHostileOrHealable);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.DynamicObject] = new TargetDefinition(
					DefaultTargetAdders.AddSelf,
					null);

			// Some poision effects which always have other targets set
			// ImplicitTargetType.EnemiesInAreaChanneledWithExceptions

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.GameObject] = new TargetDefinition(
					DefaultTargetAdders.AddObject,
					null);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.GameObjectOrItem] = new TargetDefinition(
					DefaultTargetAdders.AddItemOrObject,
					null);

			// ImplicitTargetType.HeartstoneLocation

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.InFrontOfCaster] = new TargetDefinition(
					DefaultTargetAdders.AddAreaSource,
					DefaultTargetFilters.IsInFrontAndHostile);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.ConeInFrontOfCaster] = new TargetDefinition(
					DefaultTargetAdders.AddAreaSource,
					DefaultTargetFilters.IsInFrontAndHostile);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.InvisibleOrHiddenEnemiesAtLocationRadius] = new TargetDefinition(
					DefaultTargetAdders.AddAreaSource,
					null);

			//targetHandlers[(int)ImplicitTargetType.LocationInFrontCaster] = new TargetDefinition(
			//        TargetMethods.AddAreaCaster,
			//        DefaultTargetFilters.IsInFrontFriends);

			// ImplicitTargetType.LocationInFrontCasterAtRange

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.LocationNearCaster] = new TargetDefinition(
					DefaultTargetAdders.AddSelf,
					null);

			// If not Demon and ObjectWild, add all around?
			//targetHandlers.Add(ImplicitTargetType.LocationToSummon,
			//    new TargetDefinition(
			//        TargetMethods.AddSelf,
			//        null));

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.Minion] = new TargetDefinition(
					DefaultTargetAdders.AddSelf,
					null);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.MultipleGuardianSummonLocation] = new TargetDefinition(
					DefaultTargetAdders.AddSelf,
					null);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.MultipleSummonLocation] = new TargetDefinition(
					DefaultTargetAdders.AddSelf,
					null);

			//targetHandlers[(int)ImplicitTargetType.MultipleSummonPetLocation] = new TargetDefinition(
			//        TargetMethods.AddSelf,
			//        null);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.NatureSummonLocation] = new TargetDefinition(
					DefaultTargetAdders.AddAreaDest,
					DefaultTargetFilters.IsHostile);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.NetherDrakeSummonLocation] = new TargetDefinition(
					DefaultTargetAdders.AddSelf,
					null);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.Pet] = new TargetDefinition(
					DefaultTargetAdders.AddPet,
					null);

			// default
			// TODO: What happens if items/gameobjects are involved?
			DefaultTargetHandlers[(int)ImplicitSpellTargetType.None] = new TargetDefinition(
					DefaultTargetAdders.AddSelf,
					null);

			// odd:
			DefaultTargetHandlers[(int)ImplicitSpellTargetType.PartyMember] = new TargetDefinition(
					DefaultTargetAdders.AddSelection,
					DefaultTargetFilters.IsFriendly);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.PartyAroundCaster] = new TargetDefinition(
					DefaultTargetAdders.AddAreaSource,
					DefaultTargetFilters.IsAllied);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.ScriptedOrSingleTarget] = new TargetDefinition(
					DefaultTargetAdders.AddSelection,
					DefaultTargetFilters.IsFriendly);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.SelectedEnemyChanneled] = new TargetDefinition(
					DefaultTargetAdders.AddChannelObject,
					DefaultTargetFilters.IsHostile);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.Self] = new TargetDefinition(
					DefaultTargetAdders.AddSelf,
					null);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.SingleEnemy] = new TargetDefinition(
					DefaultTargetAdders.AddSelection,
					DefaultTargetFilters.IsHostile);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.SingleFriend] = new TargetDefinition(
					DefaultTargetAdders.AddSelection,
					DefaultTargetFilters.IsFriendly);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.SingleParty] = new TargetDefinition(
					DefaultTargetAdders.AddSelection,
					DefaultTargetFilters.IsAllied);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.SpreadableDesease] = new TargetDefinition(
					DefaultTargetAdders.AddSelection,
					DefaultTargetFilters.IsHostile);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.SummonLocation] = new TargetDefinition(
					DefaultTargetAdders.AddSelf,
					null);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.TargetAtOrientationOfCaster] = new TargetDefinition(
					DefaultTargetAdders.AddAreaSource,
					DefaultTargetFilters.IsInFrontAndHostile);

			// ImplicitTargetType.TargetForVisualEffect

			// ImplicitTargetType.TeleportLocation, <- Used for Summon spells

			// Totem summoning
			DefaultTargetHandlers[(int)ImplicitSpellTargetType.TotemAir] = new TargetDefinition(
					DefaultTargetAdders.AddSelf,
					null);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.TotemEarth] = new TargetDefinition(
					DefaultTargetAdders.AddSelf,
					null);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.TotemFire] = new TargetDefinition(
					DefaultTargetAdders.AddSelf,
					null);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.TotemWater] = new TargetDefinition(
					DefaultTargetAdders.AddSelf,
					null);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.Tranquility] = new TargetDefinition(
					DefaultTargetAdders.AddAreaSource,
					DefaultTargetFilters.IsAllied);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.AllFriendlyInAura] = new TargetDefinition(
					DefaultTargetAdders.AddAreaSource,
					DefaultTargetFilters.IsAllied);
		}
		#endregion
	}
}
