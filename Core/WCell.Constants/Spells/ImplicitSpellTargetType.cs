using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.Spells
{

	public enum ImplicitSpellTargetType
	{
		/// <summary>
		/// Default
		/// </summary>
		None = 0,
		Self = 1,
		/// <summary>
		/// 30235
		/// This is cast by the Astral Flares summoned by The Curator in Karazhan.
		/// It chains to nearby characters, hitting up to three of them.
		/// </summary>
		Type_2 = 2,
		InvisibleOrHiddenEnemiesAtLocationRadius = 3,
		/// <summary>
		/// Single target, can spread to nearby. Ie 3439, 7103
		/// </summary>
		SpreadableDesease = 4,
		/// <summary>
		/// Own pet or summon
		/// </summary>
		Pet = 5,
		/// <summary>
		/// Single enemy, might be chained
		/// </summary>
		SingleEnemy = 6,
		ScriptedTarget = 7,
		AllAroundLocation = 8,
		/// <summary>
		/// Teleport back home
		/// </summary>
		HeartstoneLocation = 9,
		//Unused_Type_10 = 10,
		/// <summary>
		/// Spell 4, Word of Recall Other
		/// </summary>
		Type_11 = 11,
		//Unused_Type_12 = 12,
		//Unused_Type_13 = 13,
		//Unused_Type_14 = 14,
		AllEnemiesInArea = 15,
		AllEnemiesInAreaInstant = 16,
		TeleportLocation = 17,
		LocationToSummon = 18,
		//Unused_Type_19 = 19,
		AllPartyAroundCaster = 20,
		SingleFriend = 21,
		AllEnemiesAroundCaster = 22,
		/// <summary>
		/// All kinds of GameObject interactions (mostly for quests)
		/// </summary>
		GameObject = 23,
		/// <summary>
		/// Is only used for negative effects - for positive ones, use: LocationInFrontCaster, LocationInFrontCasterAtRange
		/// </summary>
		InFrontOfCaster = 24,
		/// <summary>
		/// Friend or Foe
		/// </summary>
		Duel = 25,
		/// <summary>
		/// Needed for all open lock spells
		/// </summary>
		GameObjectOrItem = 26,
		PetMaster = 27,
		AllEnemiesInAreaChanneled = 28,
		/// <summary>
		/// Tranquility and some Dummies (visual only)
		/// </summary>
		AllPartyInAreaChanneled = 29,
		AllFriendlyInAura = 30,
		AllTargetableAroundLocationInRadiusOverTime = 31,
		Minion = 32,
		AllPartyInArea = 33,
		/// <summary>
		/// Only used for Tranquilities' Trigger spell
		/// </summary>
		Tranquility = 34,
		SingleParty = 35,
		PetSummonLocation = 36,
		AllParty = 37,
		ScriptedOrSingleTarget = 38,
		SelfFishing = 39,
		ScriptedGameObject = 40,
		TotemEarth = 41,
		TotemWater = 42,
		TotemAir = 43,
		TotemFire = 44,
		/// <summary>
		/// Any kind of chain effect:
		/// Chain Lightning etc, as well as Multishot 
		/// </summary>
		Chain = 45,
		ScriptedObjectLocation = 46,
		/// <summary>
		/// Spell-targettype always = self
		/// Examples: Mortar, all kinds of Picnics etc
		/// </summary>
		DynamicObject = 47,
		MultipleSummonLocation = 48,
		MultipleSummonPetLocation = 49,
		SummonLocation = 50,
		/// <summary>
		/// Odd stuff
		/// </summary>
		CaliriEggs = 51,
		LocationNearCaster = 52,
		CurrentSelection = 53,
		TargetAtOrientationOfCaster = 54,
		/// <summary>
		/// Teleport location
		/// </summary>
		LocationInFrontCaster = 55,
		PartyAroundCaster = 56,
		PartyMember = 57,
		Type_58 = 58,
		TargetForVisualEffect = 59,
		ScriptedTarget2 = 60,
		AreaEffectPartyAndClass = 61,
		//Unused_PriestChampion = 62,
		NatureSummonLocation = 63,
		Type_64 = 64,
		/// <summary>
		/// Eg Shadowstep, Warp etc
		/// </summary>
		BehindTargetLocation = 65,
		Type_66 = 66,
		Type_67 = 67,
		Type_68 = 68,
		Type_69 = 69,
		Type_70 = 70,
		//Unused_Type_71 = 71,
		MultipleGuardianSummonLocation = 72,
		NetherDrakeSummonLocation = 73,
		ScriptedLocation = 74,
		/// <summary>
		/// Teleport location
		/// </summary>
		LocationInFrontCasterAtRange = 75,
		//Unused_EnemiesInAreaChanneledWithExceptions = 76,
		SelectedEnemyChanneled = 77,
		Type_78 = 78,
		Type_79 = 79,
		Type_80 = 80,
		Type_81 = 81,
		//Unused_Type_82 = 82,
		//Unused_Type_83 = 83,
		//Unused_Type_84 = 84,
		Type_85 = 85,
		SelectedEnemyDeadlyPoison = 86,
		Type_87 = 87,
		Type_88 = 88,
		Type_89 = 89,
		Type_90 = 90,
		Type_91 = 91,
		//Unused_Type_92 = 92,
		Type_93 = 93,					// Highest as of 2.4.3.8606
		ConeInFrontOfCaster = 104,
		Target_105						// Highest as of 3.2.2
	}
}
