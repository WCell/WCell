using System;
using System.Linq;
using NLog;
using WCell.Constants.NPCs;
using WCell.Constants.Pets;
using WCell.Constants.Spells;
using WCell.Core;
using WCell.Core.DBC;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.Util;
using WCell.Util.Variables;

namespace WCell.RealmServer.NPCs.Pets
{
	public static class PetMgr
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public const int MaxTotemSlots = 4;

		#region Variables & Settings & Global fields

		/// <summary>
		/// Whether players are allowed to rename pets infinitely many times
		/// </summary>
		public static bool InfinitePetRenames = false;
		public static int MinPetNameLength = 3;
		public static int MaxPetNameLength = 12;
		public static int MaxFeedPetHappinessGain = 33000;

		/// <summary>
		/// Need at least level 20 for a pet to have talents
		/// </summary>
		public static int MinPetTalentLevel = 20;

		/// <summary>
		/// Hunter pets must not have less than ownerlevel - MaxMinionLevelDifference.
		/// They gain levels if they have less.
		/// </summary>
		public static readonly int MaxHunterPetLevelDifference = 5;

		/// <summary>
		/// Percentage of character exp that pets need to level.
		/// </summary>
		public static readonly int PetExperienceOfOwnerPercent = 5;

		/// <summary>
		/// Percentage of character Stamina that gets added to the Pet's Stamina.
		/// </summary>
		public static readonly int PetStaminaOfOwnerPercent = 45;

		/// <summary>
		/// Percentage of character Armor that gets added to the Pet's Armor.
		/// </summary>
		public static readonly int PetArmorOfOwnerPercent = 35;

		/// <summary>
		/// Percentage of character RangedAttackPower that gets added to the Pet's MeleeAttackPower.
		/// </summary>
		public static readonly int PetAPOfOwnerPercent = 22;

		/// <summary>
		/// Percentage of character Resistances that get added to the Pet's Resistances.
		/// </summary>
		public static readonly int PetResistanceOfOwnerPercent = 40;

		/// <summary>
		/// Percentage of character Spell Damage that gets added to the Pet's Resistances.
		/// </summary>
		public static readonly int PetSpellDamageOfOwnerPercent = 13;

		internal static readonly NHIdGenerator PetNumberGenerator = new NHIdGenerator(typeof(PermanentPetRecord), "m_PetNumber");


		[NotVariable]
		public static int MaxStableSlots;

		[NotVariable]
		public static uint[] StableSlotPrices;
		#endregion

		#region Init
		[Initialization(InitializationPass.Sixth)]
		public static void Init()
		{
			InitMisc();
		}

		[Initialization]
		[DependentInitialization(typeof(NPCMgr))]
		public static void InitEntries()
		{
			ContentMgr.Load<PetLevelStatInfo>();
		}

		private static void InitMisc()
		{
			// Read in the prices for Stable Slots from the dbc
			var stableSlotPriceReader =
				new ListDBCReader<uint, DBCStableSlotPriceConverter>(
					RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_STABLESLOTPRICES));
			StableSlotPrices = stableSlotPriceReader.EntryList.ToArray();
			MaxStableSlots = StableSlotPrices.Length;
		}

		#endregion

		#region Naming
		public static PetNameInvalidReason ValidatePetName(ref string petName)
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
		public static uint GetStableSlotPrice(int slot)
		{
			if (slot > StableSlotPrices.Length)
			{
				return StableSlotPrices[StableSlotPrices.Length - 1];
			}
			return StableSlotPrices[slot];
		}

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
			}
			else
			{
				PetHandler.SendStableResult(chr, StableResult.Fail);
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
			}
			else
			{
				PetHandler.SendStableResult(chr, StableResult.DeStableSuccess);
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
			}
			else
			{
				PetHandler.SendStableResult(chr.Client, StableResult.BuySlotSuccess);
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
				log.Warn("Character \"{0}\" requested retreival of stabled pet from invalid NPC: {1}", chr, stableMaster);
				return false;
			}

			if (!stableMaster.CheckVendorInteraction(chr))
			{
				return false;
			}

			chr.Auras.RemoveByFlag(AuraInterruptFlags.OnStartAttack);

			return true;
		}
		#endregion

		#region Talents
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

		#region Records
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
			var mode = entry.Type == CreatureType.NonCombatPet ? PetAttackMode.Passive : PetAttackMode.Defensive;

			record.OwnerId = ownerId;
			record.AttackMode = mode;
			record.Flags = PetFlags.None;
			record.EntryId = entry.NPCId;
			return record;
		}
		#endregion
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