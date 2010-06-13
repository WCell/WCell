using System;
using System.Linq;
using NLog;
using WCell.Constants.NPCs;
using WCell.Constants.Pets;
using WCell.Constants.Spells;
using WCell.Constants.Talents;
using WCell.Core.DBC;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.NPCs.Pets;
using WCell.Util;
using WCell.Util.Variables;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.NPCs
{
	public static class PetMgr
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		#region Variables and Settings
		/// <summary>
		/// The default set of actions for every new Pet
		/// </summary>
		public static readonly PetActionEntry[] DefaultActions = new PetActionEntry[PetConstants.PetActionCount];
		public static readonly PetSpell[] EmptySpells = new PetSpell[0];

		[NotVariable]
		public static uint[] StableSlotPrices = new uint[PetConstants.MaxStableSlots];
		
		public static bool InfinitePetRenames = false;
		public static int MinPetNameLength = 3;
		public static int MaxPetNameLength = 12;
		public static int MaxFeedPetHappinessGain = 33000;

		public const int MaxTotemSlots = 4;

		[NotVariable]
		public static readonly int[] PetTalentResetPriceTiers = {
            1000, 5000, 10000, 20000, 30000, 40000, 50000, 60000,
            70000, 80000, 90000, 100000
        };

		[NotVariable]
		public static readonly int[] BaseArmorByLevel = {
               0,   20,   21,   46,   82,  126,  180,  245,  322,  412,  518,  545,
             580,  615,  650,  685,  721,  756,  791,  826,  861,  897,  932,  967,
            1002, 1037, 1072, 1108, 1142, 1177, 1212, 1247, 1283, 1317, 1353, 1387, 
            1494, 1607, 1724, 1849, 1980, 2117, 2262, 2414, 2574, 2742, 2798, 2853, 
            2907, 2963, 3018, 3072, 3128, 3183, 3237, 3292, 3348, 3402, 3457, 3512, 
            3814, 4113, 4410, 4708, 5006, 5303, 5601, 5900, 6197, 6495, 6790, 7092, 
            7392, 7692, 7992, 8292, 8592, 8892, 9192, 9492, 9792
        };

		[NotVariable]
		public static readonly int[] BaseStrengthByLevel = {
              0,  22,  23,  24,  25,  26,  27,  28,  29,  30,  31,  32,  33,  34, 
             35,  37,  38,  40,  42,  44,  45,  47,  49,  50,  52,  53,  55,  56,
             58,  60,  61,  63,  64,  66,  67,  69,  70,  72,  74,  76,  78,  81,
             86,  91,  97, 102, 104, 106, 108, 110, 113, 115, 117, 119, 122, 124,
            127, 129, 131, 134, 136, 139, 141, 144, 146, 149, 151, 154, 156, 159, 
            162, 165, 168, 171, 174, 177, 181, 184, 187, 190, 193
        };

		[NotVariable]
		public static readonly int[] BaseAgilityByLevel = {
              0,  15,  16,  16,  16,  17,  18,  18,  19,  20,  20,  20,  21,  23,  23,
             24,  25,  26,  27,  28,  30,  30,  30,  32,  33,  34,  35,  36,  37,  38,
             40,  40,  41,  43,  44,  45,  46,  47,  48,  49,  50,  52,  53,  54,  55,
             57,  57,  59,  60,  61,  63,  64,  65,  67,  68,  70,  71,  72,  74,  75, 
             77,  82,  87,  92,  97, 102, 107, 112, 117, 122, 127, 131, 136, 141, 146,
            151, 156, 161, 166, 171, 176
        };

		[NotVariable]
		public static readonly int[] BaseStaminaByLevel = {
              0,  22,  24,  25,  27,  28,  30,  32,  34,  36,  38,  40,  43,  45,  48,
             51,  54,  57,  60,  63,  66,  70,  74,  79,  83,  88,  93,  98, 103, 109,
            114, 119, 124, 129, 134, 140, 146, 152, 158, 164, 170, 177, 183, 190, 196,
            203, 210, 217, 224, 232, 240, 247, 255, 263, 271, 279, 288, 296, 305, 314,
            323, 332, 342, 351, 361, 370, 380, 391, 401, 412, 423, 434, 445, 456, 467,
            478, 489, 501, 512, 523, 534
        };

		[NotVariable]
		public static readonly int[] BaseIntelligenceByLevel = {
             0, 20, 20, 20, 20, 20, 21, 21, 21, 21, 21, 21, 21, 21, 22, 22, 22, 
            22, 22, 22, 22, 23, 23, 23, 23, 23, 23, 23, 24, 24, 24, 24, 24, 24,
            25, 25, 25, 25, 25, 26, 26, 26, 26, 26, 26, 27, 27, 27, 27, 28, 28,
            28, 28, 28, 29, 29, 29, 29, 30, 30, 30, 30, 30, 31, 31, 31, 32, 32,
            32, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43
        };

		[NotVariable]
		public static readonly int[] BaseSpiritByLevel = {
              0,  20,  20,  21,  21,  21,  21,  22,  22, 22, 23, 23, 24, 25, 26, 27, 28,  29,
             29,  31,  32,  32,  34,  34,  36,  37,  37, 39, 39, 41, 42, 42, 44, 44, 46,  47,
             48,  49,  49,  51,  52,  53,  54,  55,  56, 58, 58, 60, 60, 62, 63, 64, 65,  66,
             68,  69,  70,  71,  72,  73,  75,  78,  80, 84, 86, 88, 91, 93, 95, 96, 99, 102,
            105, 108, 111, 114, 117, 120, 123, 126, 129
        };

		/// <summary>
		/// Percentage of character exp that pets need to level.
		/// </summary>
		public static readonly int PetExperienceModifier = 25;

		/// <summary>
		/// Percentage of character Stamina that gets added to the Pet's Stamina.
		/// </summary>
		public static readonly int StaminaScaler = 45;

		/// <summary>
		/// Percentage of character Armor that gets added to the Pet's Armor.
		/// </summary>
		public static readonly int ArmorScaler = 50;

		/// <summary>
		/// Percentage of character RangedAttackPower that gets added to the Pet's MeleeAttackPower.
		/// </summary>
		public static readonly int MeleeAttackPowerScaler = 22;

		/// <summary>
		/// Percentage of character Resistances that get added to the Pet's Resistances.
		/// </summary>
		public static readonly int ResistanceScaler = 40;
		#endregion

		internal static readonly NHIdGenerator PetNumberGenerator = new NHIdGenerator(typeof(PermanentPetRecord), "m_PetNumber");

		#region Init
		[Initialization(InitializationPass.Sixth)]
		public static void Init()
		{
			// Set the Default PetActions
			var i = 0;

			DefaultActions[i++] = new PetActionEntry
			{
				Action = PetAction.Attack,
				Type = PetActionType.SetAction
			};

			DefaultActions[i++] = new PetActionEntry
			{
				Action = PetAction.Follow,
				Type = PetActionType.SetAction
			};

			DefaultActions[i++] = new PetActionEntry
			{
				Action = PetAction.Stay,
				Type = PetActionType.SetAction
			};

			DefaultActions[i++] = new PetActionEntry
			{
				Action = 0,
				Type = PetActionType.CastSpell2
			};

			DefaultActions[i++] = new PetActionEntry
			{
				Type = PetActionType.CastSpell3
			};

			DefaultActions[i++] = new PetActionEntry
			{
				Type = PetActionType.CastSpell4
			};

			DefaultActions[i++] = new PetActionEntry
			{
				Type = PetActionType.CastSpell5
			};

			DefaultActions[i++] = new PetActionEntry
			{
				AttackMode = PetAttackMode.Aggressive,
				Type = PetActionType.SetMode
			};

			DefaultActions[i++] = new PetActionEntry
			{
				AttackMode = PetAttackMode.Defensive,
				Type = PetActionType.SetMode
			};

			DefaultActions[i++] = new PetActionEntry
			{
				AttackMode = PetAttackMode.Passive,
				Type = PetActionType.SetMode
			};

			// Read in the prices for Stable Slots from the dbc
			var stableSlotPriceReader =
				new ListDBCReader<uint, DBCStableSlotPriceConverter>(
					RealmServerConfiguration.GetDBCFile("StableSlotPrices.dbc"));
			stableSlotPriceReader.EntryList.CopyTo(StableSlotPrices, 0);
		}
		#endregion

		#region Naming
		public static PetNameInvalidReason IsPetNameValid(ref string petName)
		{
			if (petName.Length == 0)
			{
				return PetNameInvalidReason.NoName;
			}

			if (petName.Length < MinPetNameLength)
			{
				return PetNameInvalidReason.TooShort;
			}

			if (petName.Length > MaxPetNameLength)
			{
				return PetNameInvalidReason.TooLong;
			}

			if (DoesNameViolate(petName))
			{
				return PetNameInvalidReason.Profane;
			}

			var spacesCount = 0;
			var apostrophesCount = 0;
			var capitalsCount = 0;
			var digitsCount = 0;
			var invalidcharsCount = 0;
			var consecutiveCharsCount = 0;
			var lastLetter = '1';
			var letterBeforeLast = '0';

			// go through character name and check for chars
			for (var i = 0; i < petName.Length; i++)
			{
				var currentChar = petName[i];
				if (!Char.IsLetter(currentChar))
				{
					switch (currentChar)
					{
						case '\'':
							apostrophesCount++;
							break;
						case ' ':
							spacesCount++;
							break;
						default:
							if (Char.IsDigit(currentChar))
							{
								digitsCount++;
							}
							else
							{
								invalidcharsCount++;
							}
							break;
					}
				}
				else
				{
					if (currentChar == lastLetter && currentChar == letterBeforeLast)
					{
						consecutiveCharsCount++;
					}
					letterBeforeLast = lastLetter;
					lastLetter = currentChar;
				}

				if (Char.IsUpper(currentChar))
				{
					capitalsCount++;
				}
			}

			if (consecutiveCharsCount > 0)
			{
				return PetNameInvalidReason.ThreeConsecutive;
			}

			if (spacesCount > 0)
			{
				return PetNameInvalidReason.ConsecutiveSpaces;
			}

			if (digitsCount > 0 || invalidcharsCount > 0)
			{
				return PetNameInvalidReason.Invalid;
			}

			if (apostrophesCount > 1)
			{
				return PetNameInvalidReason.Invalid;
			}

			// there is exactly one apostrophe
			if (apostrophesCount == 1)
			{
				var index = petName.IndexOf("'");
				if (index == 0 || index == petName.Length - 1)
				{
					// you cannot use an apostrophe as the first or last char of your name
					return PetNameInvalidReason.Invalid;
				}
			}
			// check for blizz like names flag
			if (RealmServerConfiguration.CapitalizeCharacterNames)
			{
				// do not check anything, just modify the name
				petName = petName.ToCapitalizedString();
			}

			return PetNameInvalidReason.Ok;
		}

		private static bool DoesNameViolate(string petName)
		{
			petName = petName.ToLower();

			return RealmServerConfiguration.BadWords.Where(word => petName.Contains(word)).Any();
		}
		#endregion

		#region Stables
		public static void DeStablePet(Character chr, NPC stableMaster, uint petNumber)
		{
			if (!CheckForStableMasterCheats(chr, stableMaster))
			{
				return;
			}

			var pet = chr.GetStabledPet(petNumber);
			chr.DeStablePet(pet);
			PetHandler.SendStableResult(chr, StableResult.DeStableSuccess);
		}

		public static void StablePet(Character chr, NPC stableMaster)
		{
			if (!CheckForStableMasterCheats(chr, stableMaster))
			{
				return;
			}

			var pet = chr.ActivePet;
			if (!chr.GodMode && pet.Health == 0)
			{
				PetHandler.SendStableResult(chr, StableResult.Fail);
			}

			if (chr.StabledPetRecords.Count < chr.StableSlotCount)
			{
				chr.StablePet();
				PetHandler.SendStableResult(chr, StableResult.StableSuccess);
				return;
			}
			else
			{
				PetHandler.SendStableResult(chr, StableResult.Fail);
				return;
			}
		}

		public static void SwapStabledPet(Character chr, NPC stableMaster, uint petNumber)
		{
			if (!CheckForStableMasterCheats(chr, stableMaster))
			{
				return;
			}

			var activePet = chr.ActivePet;
			var stabledPet = chr.GetStabledPet(petNumber);
			if (activePet.Health == 0)
			{
				PetHandler.SendStableResult(chr, StableResult.Fail);
				return;
			}

			if (!chr.TrySwapStabledPet(stabledPet))
			{
				PetHandler.SendStableResult(chr, StableResult.Fail);
				return;
			}
			else
			{
				PetHandler.SendStableResult(chr, StableResult.DeStableSuccess);
				return;
			}
		}

		public static void BuyStableSlot(Character chr, NPC stableMaster)
		{
			if (!CheckForStableMasterCheats(chr, stableMaster))
			{
				return;
			}

			if (!chr.TryBuyStableSlot())
			{
				PetHandler.SendStableResult(chr.Client, StableResult.NotEnoughMoney);
				return;
			}
			else
			{
				PetHandler.SendStableResult(chr.Client, StableResult.BuySlotSuccess);
				return;
			}
		}

		public static void ListStabledPets(Character chr, NPC stableMaster)
		{
			if (!CheckForStableMasterCheats(chr, stableMaster))
			{
				return;
			}

			PetHandler.SendStabledPetsList(chr, stableMaster, (byte)chr.StableSlotCount, chr.StabledPetRecords);
		}

		/// <summary>
		/// Checks StableMaster interactions for cheating.
		/// </summary>
		/// <param name="chr">The character doing the interacting.</param>
		/// <param name="stableMaster">The StableMaster the character is interacting with.</param>
		/// <returns>True if the interaction checks out.</returns>
		private static bool CheckForStableMasterCheats(Character chr, NPC stableMaster)
		{
			if (chr == null)
			{
				return false;
			}

			if (chr.GodMode)
			{
				return true;
			}

			if (!chr.IsAlive)
			{
				return false;
			}

			if (stableMaster == null || !stableMaster.IsStableMaster)
			{
				log.Warn("Client {0} requested retreival of stabled pet from invalid NPC: {1}", chr.Client, stableMaster);
				return false;
			}

			if (!stableMaster.CanInteractWith(chr))
			{
				return false;
			}

			chr.Auras.RemoveByFlag(AuraInterruptFlags.OnStartAttack);

			return true;
		}
		#endregion

		#region Talents

		public static void PetLearnTalent(Character chr, NPC pet, TalentId talentId, int rank)
		{
			if (chr == null) return;
			if (pet == null) return;
			if (chr.ActivePet != pet) return;

			SpellId spell;
			if (pet.Talents.Learn(talentId, rank, out spell))
			{
				PetHandler.SendPetLearnedSpell(chr, spell);
			}
		}

		public static void ResetPetTalents(Character chr, NPC pet)
		{
			if (chr == null) return;
			if (pet == null) return;
			if (chr.ActivePet != pet) return;

			chr.ResetPetTalents();
		}

		/// <summary>
		/// Calculates the number of base Talent points a pet should have
		///   based on its level.
		/// </summary>
		/// <param name="level">The pet's level.</param>
		/// <returns>The number of pet talent points.</returns>
		public static int GetPetTalentPointsByLevel(int level)
		{
			if (level > 19)
			{
				return (((level - 20) / 4) + 1);
			}
			return 0;
		}

		#endregion

		internal static PermanentPetRecord CreatePermanentPetRecord(NPCEntry entry, uint ownerId)
		{
			var record = CreateDefaultPetRecord<PermanentPetRecord>(entry, ownerId);
			record.PetNumber = (uint)PetNumberGenerator.Next();
			record.IsDirty = true;
			return record;
		}

		internal static T CreateDefaultPetRecord<T>(NPCEntry entry, uint ownerId)
			where T : IPetRecord, new()
		{
			var record = new T();
			var mode = entry.Type == NPCType.NonCombatPet ? PetAttackMode.Passive : PetAttackMode.Defensive;

			record.OwnerId = ownerId;
			record.AttackMode = mode;
			record.Flags = PetFlags.None;
			record.Actions = DefaultActions;
			record.EntryId = entry.NPCId;
			return record;
		}
	} // end class


	public class DBCStableSlotPriceConverter : AdvancedDBCRecordConverter<uint>
	{
		public override uint ConvertTo(byte[] rawData, ref int id)
		{
			uint currentIndex = 0;
			id = rawData.GetInt32(currentIndex++); // col 0 - "Id"
			return rawData.GetUInt32(currentIndex); // col 1 - "Cost in Copper"
		}
	} // end class
} // end namespace 
