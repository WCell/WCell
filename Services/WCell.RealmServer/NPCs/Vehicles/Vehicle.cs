using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Pets;
using WCell.RealmServer.Entities;
using WCell.Core.Network;
using WCell.Constants.NPCs;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.RealmServer.NPCs.Spawns;
using WCell.Util.Graphics;


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

		public bool HasUnitAttachment
		{
			get;
			set;
		}

		protected internal override void SetupNPC(NPCEntry entry, NPCSpawnPoint spawnPoint)
		{
			base.SetupNPC(entry, spawnPoint);
			Entry.IsIdle = true;

			NPCFlags = NPCFlags.SpellClick;
			SetupSeats();

			SetupMoveFlags();

			AddMessage(() =>
			{
				// Set Level/Scale ingame:
				var level = entry.GetRandomLevel();
				Level = level;
			});
		}

		private void SetupMoveFlags()
		{
			var flags = Entry.VehicleEntry.Flags;
			if(flags.HasAnyFlag(VehicleFlags.PreventJumping))
			{
				MovementFlags2 |= MovementFlags2.PreventJumping;
			}
			if (flags.HasAnyFlag(VehicleFlags.PreventStrafe))
			{
				MovementFlags2 |= MovementFlags2.PreventStrafe;
			}
			if (flags.HasAnyFlag(VehicleFlags.FullSpeedTurning))
			{
				MovementFlags2 |= MovementFlags2.FullSpeedTurning;
			}
			if (flags.HasAnyFlag(VehicleFlags.AlwaysAllowPitching))
			{
				MovementFlags2 |= MovementFlags2.AlwaysAllowPitching;
			}
			if (flags.HasAnyFlag(VehicleFlags.FullSpeedPitching))
			{
				MovementFlags2 |= MovementFlags2.FullSpeedPitching;
			}
		}

		private void SetupSeats()
		{
			var entries = m_entry.VehicleEntry.Seats;
			m_Seats = new VehicleSeat[entries.Length];

			for (var i = 0; i < entries.Length; i++)
			{
				var entry = entries[i];
				if (entry == null) continue;

				m_Seats[i] = new VehicleSeat(this, entry, (byte)i);
				if (m_Seats[i].Entry.PassengerNPCId == 0) continue;
				HasUnitAttachment = true;
				m_Seats[i].CharacterCanEnterOrExit = false;

				//copy locally (access to modified closure)
				var seat = i;
				AddMessage(() =>
				           	{
				           		var npcEntry = NPCMgr.GetEntry(entry.PassengerNPCId);
				           		if (npcEntry == null)
				           			return;

				           		var newNpc = npcEntry.SpawnAt(this);
				           		newNpc.Brain.EnterDefaultState();
				           		m_Seats[seat].Enter(newNpc);
				           	});
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

		public override bool SetPosition(Vector3 pt)
		{
			var res = m_Map.MoveObject(this, ref pt);
			foreach(var seat in m_Seats.Where(seat => seat != null && seat.Passenger != null))
			{
				res = seat.Passenger.SetPosition(pt + seat.Entry.AttachmentOffset);
			}
			return res;
		}

		public override bool SetPosition(Vector3 pt, float orientation)
		{
			if (m_Map.MoveObject(this, ref pt))
			{
				m_orientation = orientation;
				var res = true;
				foreach (var seat in m_Seats.Where(seat => seat != null && seat.Passenger != null))
				{
					res = seat.Passenger.SetPosition(pt + seat.Entry.AttachmentOffset);
					seat.Passenger.Orientation = orientation + seat.Entry.PassengerYaw;
				}
				return res;
			}
			return false;
		}

		public bool CanEnter(Unit unit)
		{
            return IsAtLeastNeutralWith(unit) && !IsFull;
		}

		public VehicleSeat GetFirstFreeSeat(bool isCharacter)
		{
			for (var i = 0; i < m_Seats.Length; i++)
			{
				var seat = m_Seats[i];
				if(seat == null || (isCharacter && !seat.CharacterCanEnterOrExit))
					continue;

				if (!seat.IsOccupied)
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
			if (!CanEnter(unit))
			{
				return null;
			}
			return GetFirstFreeSeat(unit is Character);
		}

		public void ClearAllSeats(bool onlyClearUsableSeats = false)
		{
			foreach (var seat in m_Seats)
			{
				if (seat != null && (!onlyClearUsableSeats || seat.CharacterCanEnterOrExit))
				{
					seat.ClearSeat();
				}
			}

            Dismiss();
		}

        public void Dismiss()
        {
			if (Entry.VehicleEntry.IsMinion)
				Delete();
			else
			{
				//TODO: Return to spawn point, without causing exceptions!
			}
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
			if (HasUnitAttachment)
			{
				foreach (var seat in Seats.Where(seat => seat != null))
				{
					if (seat.Passenger != null && seat.HasUnitAttachment)
						seat.Passenger.Delete();
				}
			}

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