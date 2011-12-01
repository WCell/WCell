using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.NPCs;
using WCell.Constants.Pets;
using WCell.Core.ClientDB;
using WCell.RealmServer.Skills;
using WCell.RealmServer.Spells;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.NPCs
{
	#region CreatureFamily.dbc
	public class DBCCreatureFamilyConverter : AdvancedClientDBRecordConverter<CreatureFamily>
	{
		public override CreatureFamily ConvertTo(byte[] rawData, ref int id)
		{
			var family = new CreatureFamily {
				Id = ((CreatureFamilyId)(id = (int)GetUInt32(rawData, 0))),
				MinScale = GetFloat(rawData, 1),
				MaxScale = GetFloat(rawData, 3),
				MaxScaleLevel = GetInt32(rawData, 4),
				SkillLine = SkillHandler.Get(GetUInt32(rawData, 5)),
				PetFoodMask = (PetFoodMask)GetUInt32(rawData, 7),
				PetTalentType = (PetTalentType)GetUInt32(rawData, 8),
				Name = GetString(rawData, 10)
			};

			family.ScaleStep = (family.MaxScale - family.MinScale) / family.MaxScaleLevel;
		    return family;
		}
	}
	#endregion

	#region CreatureSpellData.dbc
	public class DBCCreatureSpellConverter : AdvancedClientDBRecordConverter<Spell[]>
	{
		public override Spell[] ConvertTo(byte[] rawData, ref int id)
		{
			var spells = new List<Spell>(4);

			id = (int)GetUInt32(rawData, 0);

			Spell spell;
			for (int i = 1; i <= 4; i++)
			{
				var spellId = GetUInt32(rawData, i);
				if (spellId != 0)
				{
					if ((spell = SpellHandler.Get(spellId)) != null)
					{
						spells.Add(spell);
					}
				}
			}

			return spells.ToArray();
		}
	}
	#endregion

	#region BankBagSlotPrices
	public class DBCBankBagSlotConverter : AdvancedClientDBRecordConverter<uint>
	{
		public override uint ConvertTo(byte[] rawData, ref int id)
		{
			List<Spell> spells = new List<Spell>(4);

			id = (int)GetUInt32(rawData, 0);

			return rawData.GetUInt32(1);
		}
	}
	#endregion

    #region Vehicle
    public class DBCVehicleConverter : AdvancedClientDBRecordConverter<VehicleEntry> 
    {
        public override VehicleEntry ConvertTo(byte[] rawData, ref int id) {
            id = GetInt32(rawData, 0);
            var vehicle = new VehicleEntry {
                Id = GetUInt32(rawData,0),
                Flags = (VehicleFlags)GetUInt32(rawData,1),
                TurnSpeed = GetFloat(rawData,2),
                PitchSpeed = GetFloat(rawData,3),
                PitchMin = GetFloat(rawData,4),
                PitchMax = GetFloat(rawData,5)
            };

        	var lastSeatId = 0;
        	var count = 0;
			for (var i = 0; i < vehicle.Seats.Length; i++)
			{
				var seatId = GetUInt32(rawData, 6 + i);
				if (seatId <= 0) continue;

				var seatEntry = NPCMgr.GetVehicleSeatEntry(seatId);
				vehicle.Seats[i] = seatEntry;
				count++;
				lastSeatId = i;
			}

			vehicle.SeatCount = count;
			if (lastSeatId < 7)
			{
				Array.Resize(ref vehicle.Seats, (int)lastSeatId + 1);
			}

        	vehicle.PowerType = (VehiclePowerType)GetInt32(rawData, 37);

        	return vehicle;
        }
    }

    public class DBCVehicleSeatConverter : AdvancedClientDBRecordConverter<VehicleSeatEntry> 
    {
        public override VehicleSeatEntry ConvertTo(byte[] rawData, ref int id) {
            id = GetInt32(rawData, 0);
            var seat = new VehicleSeatEntry {
				Id = GetUInt32(rawData, 0),
                Flags = (VehicleSeatFlags)GetUInt32(rawData,1),
                AttachmentId = GetInt32(rawData,2),
                AttachmentOffset = new Vector3(
					GetFloat(rawData,3),
					GetFloat(rawData,4),
					GetFloat(rawData,5)),
                EnterPreDelay = GetFloat(rawData,6),
                EnterSpeed = GetFloat(rawData,7),
                EnterGravity = GetFloat(rawData,8),
                EnterMinDuration = GetFloat(rawData,9),
                EnterMaxDuration = GetFloat(rawData,10),
                EnterMinArcHeight = GetFloat(rawData,11),
                EnterMaxArcHeight = GetFloat(rawData,12),
                EnterAnimStart = GetInt32(rawData,13),
                EnterAnimLoop = GetInt32(rawData,14),
                RideAnimStart = GetInt32(rawData,15),
                RideAnimLoop = GetInt32(rawData,16),
                RideUpperAnimStart = GetInt32(rawData,17),
                RideUpperAnimLoop = GetInt32(rawData,18),
                ExitPreDelay = GetFloat(rawData,19),
                ExitSpeed = GetFloat(rawData,20),
                ExitGravity = GetFloat(rawData,21),
                ExitMinDuration = GetFloat(rawData,22),
                ExitMaxDuration = GetFloat(rawData,23),
                ExitMinArcHeight = GetFloat(rawData,24),
                ExitMaxArcHeight = GetFloat(rawData,25),
                ExitAnimStart = GetInt32(rawData,26),
                ExitAnimLoop = GetInt32(rawData,27),
                ExitAnimEnd = GetInt32(rawData,28),
                PassengerYaw = GetFloat(rawData,29),
                PassengerPitch = GetFloat(rawData,30),
                PassengerRoll = GetFloat(rawData,31),
                PassengerAttachmentId = GetInt32(rawData,32),
				FlagsB = (VehicleSeatFlagsB)GetUInt32(rawData, 45)
            };
            return seat;
        }
    }
    #endregion

	#region Barbershops

	public class BarberShopStyleEntry
	{
		public int Id;
		public int Type;
		// These are unused, currently.
		// string Name;
		// uint NameFlags;
		// uint UnkName;
		// uint UnkFlags;
		// float CostMultiplier;
		public RaceId Race;
		public GenderType Gender;
		public int HairId;
	}

	public class BarberShopStyleConverter : AdvancedClientDBRecordConverter<BarberShopStyleEntry>
	{
		public override BarberShopStyleEntry ConvertTo(byte[] rawData, ref int id)
		{
            var barberShopStyleEntry = new BarberShopStyleEntry();

			barberShopStyleEntry.Id = GetInt32(rawData, 0);
			barberShopStyleEntry.Type = GetInt32(rawData, 1);
            //string name 2
            //uint UnkName 3
            //float cost multiplier 4
			barberShopStyleEntry.Race = (RaceId)GetUInt32(rawData, 5);
			barberShopStyleEntry.Gender = (GenderType)GetUInt32(rawData, 6);
			barberShopStyleEntry.HairId = GetInt32(rawData, 7);

			return barberShopStyleEntry;
		}
	}

	#endregion
}
