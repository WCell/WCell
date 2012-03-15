using System.Linq;
using WCell.Constants;
using WCell.Constants.NPCs;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs.Spawns;
using WCell.Util.Graphics;

namespace WCell.RealmServer.NPCs.Vehicles
{
    public class Vehicle : NPC, ITransportInfo
    {
        private VehicleSeat[] _seats;
        protected internal int _passengerCount;

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

            NPCFlags = NPCFlags.SpellClick;
            SetupSeats();

            SetupMoveFlags();

            AddMessage(() =>
            {
                // Set Level/Scale ingame:
                var level = entry.GetRandomLevel();
                Level = level;
                PowerType = PowerType.Energy;
                MaxPower = entry.VehicleEntry.PowerType == VehiclePowerType.Pyrite ? 50 : 100;
                Power = MaxPower;
                if (entry.Spells == null)
                    PowerType = PowerType.End;
            });
        }

        private void SetupMoveFlags()
        {
            var flags = Entry.VehicleEntry.Flags;
            if (flags.HasAnyFlag(VehicleFlags.PreventJumping))
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
            _seats = new VehicleSeat[entries.Length];

            for (var i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                if (entry == null) continue;

                _seats[i] = new VehicleSeat(this, entry, (byte)i);
                if (_seats[i].Entry.PassengerNPCId == 0) continue;
                HasUnitAttachment = true;
                _seats[i].CharacterCanEnterOrExit = false;

                //copy locally (access to modified closure)
                var seat = i;
                AddMessage(() =>
                            {
                                var npcEntry = NPCMgr.GetEntry(entry.PassengerNPCId);
                                if (npcEntry == null)
                                    return;

                                var newNpc = npcEntry.SpawnAt(this);
                                newNpc.Brain.EnterDefaultState();
                                _seats[seat].Enter(newNpc);
                            });
            }
        }

        public int PassengerCount
        {
            get { return _passengerCount; }
        }

        public int SeatCount
        {
            get { return m_entry.VehicleEntry.SeatCount; }
        }

        public int FreeSeats
        {
            get { return m_entry.VehicleEntry.SeatCount - _passengerCount; }
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
            get { return _seats; }
        }

        public Unit Driver
        {
            get { return _seats[0].Passenger; }
        }

        public override bool SetPosition(Vector3 pt)
        {
            var res = m_Map.MoveObject(this, ref pt);
            foreach (var seat in _seats.Where(seat => seat != null && seat.Passenger != null))
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
                foreach (var seat in _seats.Where(seat => seat != null && seat.Passenger != null))
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
            for (var i = 0; i < _seats.Length; i++)
            {
                var seat = _seats[i];
                if (seat == null || (isCharacter && !seat.CharacterCanEnterOrExit))
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
            foreach (var seat in _seats)
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

        /// <summary>
        /// Returns null if the passenger could not be found
        /// </summary>
        public VehicleSeat FindSeatOccupiedBy(EntityId entityId)
        {
            return Seats.Where(vehicleSeat => vehicleSeat != null && vehicleSeat.Passenger != null && vehicleSeat.Passenger.EntityId == entityId).FirstOrDefault();
        }

        /// <summary>
        /// Returns null if the unit could not be found
        /// </summary>
        public VehicleSeat FindSeatOccupiedBy(Unit passenger)
        {
            return Seats.Where(vehicleSeat => vehicleSeat != null && vehicleSeat.Passenger != null && vehicleSeat.Passenger == passenger).FirstOrDefault();
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

        #endregion Updates
    }
}