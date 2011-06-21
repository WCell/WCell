using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Pets;
using WCell.RealmServer.Entities;
using WCell.Core.Network;
using WCell.Constants.NPCs;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.RealmServer.NPCs.Spawns;


namespace WCell.RealmServer.NPCs.Vehicles
{
	public class Vehicle : NPC, ITransportInfo
	{
		private VehicleSeat[] m_Seats;
		protected internal int m_passengerCount;

		public Vehicle()
		{
		}

		protected override HighId HighId
		{
			get { return HighId.Vehicle; }
		}

		protected internal override void SetupNPC(NPCEntry entry, NPCSpawnPoint spawnPoint)
		{
			base.SetupNPC(entry, spawnPoint);

			NPCFlags = NPCFlags.SpellClick;
			SetupSeats();

			AddMessage(() =>
			{
				// Set Level/Scale ingame:
				var level = entry.GetRandomLevel();
				Level = level;
			});
		}

		private void SetupSeats()
		{
			var entries = m_entry.VehicleEntry.Seats;
			m_Seats = new VehicleSeat[entries.Length];

			var driver = true;
			for (var i = 0; i < entries.Length; i++)
			{
				var entry = entries[i];
				if (entry != null)
				{
					m_Seats[i] = new VehicleSeat(this, entry, (byte)i, driver);
					driver = false;
				}
			}
		}

		public int PassengerCount
		{
			get { return m_passengerCount; }
		}

		public int SeatCount
		{
			get { return m_entry.VehicleEntry.SeatCount; }
		}

		public int FreeSeats
		{
			get { return m_entry.VehicleEntry.SeatCount - m_passengerCount; }
		}

		public bool IsFull
		{
			get
			{
				return FreeSeats < 1;
			}
		}

		public VehicleSeat[] Seats
		{
			get { return m_Seats; }
		}

		public Unit Driver
		{
			get { return m_Seats[0].Passenger; }
		}

		public bool CanEnter(Unit unit)
		{
            return IsNeutralWith(unit) && !IsFull;
		}

		public VehicleSeat GetFirstFreeSeat()
		{
			for (var i = 0; i < m_Seats.Length; i++)
			{
				var seat = m_Seats[i];
				if (seat != null && !seat.IsOccupied)
				{
					return seat;
				}
			}
			return null;
		}

		/// <summary>
		/// Returns null if unit may not enter or there is no free seat available
		/// </summary>
		public VehicleSeat GetSeatFor(Unit unit)
		{
			if (CanEnter(unit))
			{
				return GetFirstFreeSeat();
			}
			return null;
		}

		public void ClearAllSeats()
		{
			foreach (var seat in m_Seats)
			{
				if (seat != null)
				{
					seat.ClearSeat();
				}
			}

            if (Entry.VehicleEntry.IsMinion)
                Dismiss();
            else
            {
                //TODO: Return to spawn point, without causing exceptions!
            }
		}

        public void Dismiss()
        {
            RemoveFromMap();
        }

        public uint[] BuildVehicleActionBar()
        {
            var bar = new uint[PetConstants.PetActionCount];
            var i = 0;

            byte j = 0;
            if (Entry.Spells != null)
            {
                var spells = Entry.Spells.GetEnumerator();

                for (j = 0; j < PetConstants.PetSpellCount; j++)
                {
                    if (!spells.MoveNext())
                    {
                        bar[i++] = new PetActionEntry
                        {
                            Type = PetActionType.CastSpell2 + j
                        }.Raw;
                    }
                    else
                    {
                        var spell = spells.Current;
                        var actionEntry = new PetActionEntry();
                        if (spell.Value.IsPassive)
                        {
                            var cast = SpellCast;
                            if (cast != null)
                                cast.TriggerSelf(spell.Value);

                            actionEntry.Type = PetActionType.CastSpell2 + j;
                        }
                        else
                        {
                            actionEntry.SetSpell(spell.Key, PetActionType.DefaultSpellSetting);

                        }
                        bar[i++] = actionEntry.Raw;
                    }
                }
            }
            else
            {
                for (j = 0; j < PetConstants.PetSpellCount; j++)
                {
                    bar[i++] = new PetActionEntry
                    {
                        Type = PetActionType.CastSpell2 + j
                    }.Raw;
                }
            }

            for (; j < PetConstants.PetActionCount; j++)
            {
                bar[i++] = new PetActionEntry
                {
                    Type = PetActionType.CastSpell2 + j
                }.Raw;
            }

            return bar;
        }

		protected internal override void DeleteNow()
		{
			ClearAllSeats();
			base.DeleteNow();
		}

		#region Updates
		public override UpdateFlags UpdateFlags
		{
			get
			{
				return UpdateFlags.Flag_0x10 | UpdateFlags.Living | UpdateFlags.StationaryObject | UpdateFlags.Vehicle;
			}
		}

		protected override void WriteTypeSpecificMovementUpdate(PrimitiveWriter writer, UpdateFieldFlags relation, UpdateFlags updateFlags)
		{
			base.WriteTypeSpecificMovementUpdate(writer, relation, updateFlags);
			writer.Write(m_entry.VehicleId);
			writer.Write(m_entry.VehicleAimAdjustment);
		}
		#endregion
	}
}