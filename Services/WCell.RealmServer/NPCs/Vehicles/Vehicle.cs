using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
				return UpdateFlags.Living | UpdateFlags.StationaryObject | UpdateFlags.Vehicle;
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