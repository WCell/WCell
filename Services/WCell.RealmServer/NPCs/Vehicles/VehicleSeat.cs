using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.RealmServer.AI;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.Core;

namespace WCell.RealmServer.NPCs.Vehicles
{
	public class VehicleSeat
	{
		public readonly Vehicle Vehicle;
		public readonly VehicleSeatEntry Entry;
		public readonly byte Index;
		private Unit _passenger;

		public bool IsDriverSeat
		{
			get { return Entry.Flags.HasAnyFlag(VehicleSeatFlags.VehicleControlSeat); }
		}

		public bool CharacterCanEnterOrExit
		{
			get { return Entry.Flags.HasAnyFlag(VehicleSeatFlags.CanEnterorExit); }
			internal set
			{ 
				if(!value)
					Entry.Flags &= ~VehicleSeatFlags.CanEnterorExit;
				else
					Entry.Flags |= VehicleSeatFlags.CanEnterorExit;
			}
		}

		public bool HasUnitAttachment
		{
			get { return Entry.PassengerNPCId != 0; }
		}

		public VehicleSeat(Vehicle vehicle, VehicleSeatEntry entry, byte index)
		{
			Vehicle = vehicle;
			Entry = entry;
			Index = index;
		}

		public bool IsOccupied
		{
			get { return _passenger != null; }
		}

		public Unit Passenger
		{
			get { return _passenger; }
			internal set { _passenger = value; }
		}

		/// <summary>
		/// Add Passenger
		/// </summary>
		public void Enter(Unit passenger)
		{
			this._passenger = passenger;

			passenger.m_vehicleSeat = this;
			passenger.MovementFlags |= MovementFlags.OnTransport;
			passenger.TransportPosition = Entry.AttachmentOffset;
			passenger.TransportOrientation = Entry.PassengerYaw;
			Vehicle.m_passengerCount++;

			passenger.IncMechanicCount(SpellMechanic.Rooted);

			if (!(passenger is Character)) return;

			if (IsDriverSeat)
			{
				Vehicle.Charmer = passenger;
				passenger.Charm = Vehicle;
				Vehicle.UnitFlags |= UnitFlags.Possessed;
				//Force idle so the vehicle doesn't start trying to
				//attack while we are in control
				Vehicle.Brain.State = BrainState.Idle;
			}

			var chr = (Character)passenger;
			var pos = Vehicle.Position;

			VehicleHandler.Send_SMSG_ON_CANCEL_EXPECTED_RIDE_VEHICLE_AURA(chr);
			VehicleHandler.SendBreakTarget(chr, Vehicle);

			MovementHandler.SendEnterTransport(chr);
			MiscHandler.SendCancelAutoRepeat(chr, Vehicle);
			MovementHandler.SendMoveToPacket(Vehicle, ref pos, 0, 0, MonsterMoveFlags.Walk);
			PetHandler.SendVehicleSpells(chr, Vehicle);

			chr.SetMover(Vehicle, IsDriverSeat);
			chr.FarSight = Vehicle.EntityId;
		}

		/// <summary>
		/// Remove Passenger
		/// </summary>
		public void ClearSeat()
		{
			if (_passenger == null)
			{
				return;
			}

			if (IsDriverSeat)
			{
				Vehicle.Charmer = null;
				_passenger.Charm = null;
				Vehicle.UnitFlags &= ~UnitFlags.Possessed;
			}

			Vehicle.m_passengerCount--;
            if (_passenger.MovementFlags.HasFlag(MovementFlags.Flying))
            {
                var cast = Vehicle.SpellCast;
                    if(cast != null)
                        cast.Trigger(SpellId.EffectParachute);
            }

		    _passenger.MovementFlags &= ~MovementFlags.OnTransport;
			_passenger.Auras.RemoveFirstVisibleAura(aura => aura.Spell.IsVehicle);

			if (_passenger is Character)
			{
				var chr = (Character)_passenger;
				VehicleHandler.Send_SMSG_ON_CANCEL_EXPECTED_RIDE_VEHICLE_AURA(chr);
                //SendTeleportAck
                MovementHandler.SendMoved(chr);
				MiscHandler.SendCancelAutoRepeat(chr, Vehicle);
				chr.ResetMover();
				chr.FarSight = EntityId.Zero;

				//MovementHandler.SendEnterTransport(chr);
			}
            _passenger.m_vehicleSeat = null;
		    MovementHandler.SendHeartbeat(_passenger, _passenger.Position, _passenger.Orientation);
            _passenger.DecMechanicCount(SpellMechanic.Rooted);
			_passenger = null;
		}
	}
}