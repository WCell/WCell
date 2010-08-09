using System;
using System.Collections.Generic;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Misc;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Pets;
using WCell.Util;
using WCell.Util.Data;
using WCell.Util.Variables;

namespace WCell.RealmServer.Formulas
{
	/// <summary>
	/// Takes Target-level and receiver-level and returns the amount of base-experience to be gained
	/// </summary>
	public delegate int BaseExperienceCalculator(int targetLvl, int receiverLvl);

	public delegate int ExperienceCalculator(int receiverLvl, NPC npc);

	/// <summary>
	/// </summary>
	/// <returns>The amount of tequired Experience for that level</returns>
	public delegate int XpCalculator(int level);

	[DataHolder]
	public class LevelXp : IDataHolder
	{
		public int Level, Xp;

		public uint GetId()
		{
			return (uint)Level;
		}

		public DataHolderState DataHolderState
		{
			get;
			set;
		}

		public void FinalizeDataHolder()
		{
			ArrayUtil.Set(ref XpGenerator.XpTable, (uint)Level, Xp);
		}
	}

	/// <summary>
	/// Static utility class that holds and calculates Level- and Experience-information.
	/// Has exchangable Calculator delegates to allow custom xp-calculations.
	/// </summary>
	public static class XpGenerator
	{
		/// <summary>
		/// The base-factor per level for calculations of XP given by killing NPCs.
		/// </summary>
		public static int DefaultXpLevelFactor = 5;

		/// <summary>
		/// The default base value for Xp-calculations from NPCs in the old land.
		/// </summary>
		public static int DefaultXpBaseValueAzeroth = 45;

		/// <summary>
		/// The default base value for Xp-calculations from NPCs in Outland.
		/// </summary>
		public static int DefaultXpBaseValueOutland = 235;

		/// <summary>
		/// The factor to be applied to Xp that is gained when exploring a new Zone
		/// </summary>
		public static int ExplorationXpFactor = 1;

		/// <summary>
		/// If the level of a Character is greater than what is defined in the <see cref="XpTable"/> and yet smaller than
		/// the <see cref="RealmServerConfiguration.MaxCharacterLevel"/>, this function will be called to return the experience to gain on that given level.
		/// </summary>
		public static XpCalculator MaxLevelXpCalculator = (level) => 1000 * level;

		/// <summary>
		/// Change this method in addons to create custom XP calculation
		/// </summary>
		public static Action<Character, INamed, int> CombatXpDistributer = DistributeCombatXp;

		[Initialization(InitializationPass.Fifth, "Initialize Experience-Table")]
		public static void Initialize()
		{
			ContentHandler.Load<LevelXp>();
		}

		/// <summary>
		/// Retrieves the amount of xp from the xp-table.
		/// This covers Experience for all physically possible levels (between 0 and 255).
		/// </summary>
		//public static readonly BaseExperienceCalculator CachedCalculator =(targetLvl, receiverLvl) => Experience.XpTable[targetLvl, receiverLvl];

		/// <summary>
		/// Takes the base-formular to calculate the Xp to be gained for the receiver.
		/// </summary>
		//public static readonly BaseExperienceCalculator DefaultCalculator = ;

		public static int CalcDefaultBaseXp(int targetLevel, int receiverLvl)
		{
			return CalcXp(targetLevel, receiverLvl, DefaultXpLevelFactor, DefaultXpBaseValueAzeroth);
		}

		public static int CalcOutlandBaseXp(int targetLevel, int receiverLvl)
		{
			return CalcXp(targetLevel, receiverLvl, DefaultXpLevelFactor, DefaultXpBaseValueOutland);
		}

		public static int CalcDefaultXp(int targetLevel, NPC npc)
		{
			return CalcXp(npc.Level, targetLevel, DefaultXpLevelFactor, DefaultXpBaseValueAzeroth);
			// TODO: Extra calcs
		}

		public static int CalcOutlandXp(int targetLevel, NPC npc)
		{
			return CalcXp(npc.Level, targetLevel, DefaultXpLevelFactor, DefaultXpBaseValueOutland);
			// TODO: Extra calcs
		}

		public static int CalcXp(int targetLvl, int receiverLvl, int factor, int baseValue)
		{
			var xp = (receiverLvl * factor) + baseValue;

			if (targetLvl >= receiverLvl)
			{
				xp += MathUtil.Divide(xp * (targetLvl - receiverLvl), 20);
			}
			else
			{
				int grayLevel;
				if (receiverLvl > 39)
				{
					grayLevel = receiverLvl - 1 - MathUtil.Divide(receiverLvl, 5);
				}
				else if (receiverLvl > 5)
				{
					grayLevel = receiverLvl - 5 - MathUtil.Divide(receiverLvl, 10);
				}
				else
				{
					grayLevel = 0;
				}

				if (targetLvl > grayLevel)
				{
					int zeroDiff;
					if (receiverLvl > 39)
					{
						zeroDiff = 5 + MathUtil.Divide(receiverLvl, 5);
					}
					else if (receiverLvl > 19)
					{
						zeroDiff = 9 + MathUtil.Divide(receiverLvl, 10);
					}
					else if (receiverLvl > 15)
					{
						zeroDiff = 9;
					}
					else if (receiverLvl > 11)
					{
						zeroDiff = 8;
					}
					else if (receiverLvl > 9)
					{
						zeroDiff = 7;
					}
					else if (receiverLvl > 7)
					{
						zeroDiff = 6;
					}
					else
					{
						zeroDiff = 5;
					}
					xp += MathUtil.Divide((xp * (1 - (receiverLvl - targetLvl))), zeroDiff);
				}
				else
				{
					xp = 0;
				}
			}
			return Math.Max(0, xp);
		}

		public static int GetExplorationXp(ZoneTemplate zone, Character character)
		{
			return ExplorationXpFactor * (zone.AreaLevel * 20);
		}

		/// <summary>
		/// Distributes the given amount of XP over the group of the given Character (or adds it only to the Char, if not in Group).
		/// </summary>
		/// <remarks>Requires Region-Context.</remarks>
		/// <param name="chr"></param>
		public static void DistributeCombatXp(Character chr, INamed killed, int xp)
		{
			var groupMember = chr.GroupMember;
			if (groupMember != null)
			{
				var members = new List<Character>();
				var highestLevel = 0;
				var totalLevels = 0;
				groupMember.IterateMembersInRange(WorldObject.BroadcastRange,
					member => {
						var memberChar = member.Character;
						if (memberChar != null)
						{
							totalLevels += memberChar.Level;
							if (memberChar.Level > highestLevel)
							{
								highestLevel = memberChar.Level;
							}
							members.Add(memberChar);
						}
					});

				foreach (var member in members)
				{
					var share = MathUtil.Divide(xp * member.Level, totalLevels);
					member.GainCombatXp(share, killed, true);
				}
			}
			else
			{
				chr.GainCombatXp(xp, killed, true);
			}
		}

		/// <summary>
		/// Gets the amount of xp, required to gain this level (from level-1)
		/// </summary>
		public static int GetXpForlevel(int level)
		{
			if (XpTable.Length >= level)
			{
				return XpTable[level - 1];
			}
			if (level < RealmServerConfiguration.MaxCharacterLevel)
			{
				return MaxLevelXpCalculator(level);
			}
			return 0;
		}

		public static int GetPetXPForLevel(int level)
		{
			return ((GetXpForlevel(level) * PetMgr.PetExperienceModifier) / 100);
		}

		public static int GetGrayLevel(int playerLevel)
		{
			if (playerLevel <= 5) return 0;
			else if (playerLevel <= 39) return playerLevel - 5 - playerLevel / 10;
			else if (playerLevel <= 59) return playerLevel - 1 - playerLevel / 5;
			else return playerLevel - 9;
		}

		[NotVariable]
		/// <summary>
		/// Array of Xp to be gained per level for default levels.
		/// Can be set to a different Array.
		/// </summary>
		public static int[] XpTable = new[] 
                                              { 
                                                  0,      // XP to level 1
                                                  400,    // XP to level 2
                                                  900,    // XP to level 3
                                                  1400,   // XP to level 4
                                                  2100,   // XP to level 5
                                                  2800,   // XP to level 6
                                                  3600,   // XP to level 7
                                                  4500,   // XP to level 8
                                                  5400,   // XP to level 9
                                                  6500,   // XP to level 10
                                                  7600,   // XP to level 11
                                                  8700,   // XP to level 12
                                                  9800,   // XP to level 13
                                                  11000,  // XP to level 14
                                                  12300,  // XP to level 15
                                                  13600,  // XP to level 16
                                                  15000,  // XP to level 17
                                                  16400,  // XP to level 18
                                                  17800,  // XP to level 19
                                                  19300,  // XP to level 20
                                                  20800,  // XP to level 21
                                                  22400,  // XP to level 22
                                                  24000,  // XP to level 23
                                                  25500,  // XP to level 24
                                                  27200,  // XP to level 25
                                                  28900,  // XP to level 26
                                                  30500,  // XP to level 27
                                                  32200,  // XP to level 28
                                                  33900,  // XP to level 29
                                                  36300,  // XP to level 30
                                                  38800,  // XP to level 31
                                                  41600,  // XP to level 32
                                                  44600,  // XP to level 33
                                                  48000,  // XP to level 34
                                                  51400,  // XP to level 35
                                                  55000,  // XP to level 36
                                                  58700,  // XP to level 37
                                                  62400,  // XP to level 38
                                                  66200,  // XP to level 39
                                                  70200,  // XP to level 40
                                                  74300,  // XP to level 41
                                                  78500,  // XP to level 42
                                                  82800,  // XP to level 43
                                                  87100,  // XP to level 44
                                                  91600,  // XP to level 45
                                                  95300,  // XP to level 46
                                                  101000, // XP to level 47
                                                  105800, // XP to level 48
                                                  110700, // XP to level 49
                                                  115700, // XP to level 50
                                                  120900, // XP to level 51
                                                  126100, // XP to level 52
                                                  131500, // XP to level 53
                                                  137000, // XP to level 54
                                                  142500, // XP to level 55
                                                  148200, // XP to level 56
                                                  154000, // XP to level 57
                                                  159900, // XP to level 58
                                                  165800, // XP to level 59
                                                  172000, // XP to level 60
                                                  290000, // XP to level 61
                                                  317000, // XP to level 62
                                                  349000, // XP to level 63
                                                  386000, // XP to level 64
                                                  428000, // XP to level 65
                                                  475000, // XP to level 66
                                                  527000, // XP to level 67
                                                  585000, // XP to level 68
                                                  648000, // XP to level 69
                                                  717000,  // XP to level 70
                                                  1523800, // XP to level 71
                                                  1539600, // XP to level 72
                                                  1555700, // XP to level 73
                                                  1571800, // XP to level 74
                                                  1587900, // XP to level 75
                                                  1604200, // XP to level 76
                                                  1620700, // XP to level 77
                                                  1637400, // XP to level 78
                                                  1653900, // XP to level 79
                                                  1670800, // XP to level 80
                                              };
	}
}