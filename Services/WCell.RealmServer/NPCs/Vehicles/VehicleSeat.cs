using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
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
		public readonly bool IsDriverSeat;
		private Unit m_passenger;

		public VehicleSeat(Vehicle vehicle, VehicleSeatEntry entry, byte index, bool driver)
		{
			Vehicle = vehicle;
			Entry = entry;
			Index = index;
			IsDriverSeat = driver;
		}

		public bool IsOccupied
		{
			get { return m_passenger != null; }
		}

		public Unit Passenger
		{
			get { return m_passenger; }
			internal set { m_passenger = value; }
		}

		/// <summary>
		/// Add Passenger
		/// </summary>
		public void Enter(Unit passenger)
		{
			m_passenger = passenger;

			passenger.m_vehicleSeat = this;
			passenger.MovementFlags |= MovementFlags.OnTransport;
			passenger.TransportPosition = Entry.AttachmentOffset;
			passenger.TransportOrientation = Entry.PassengerYaw;
			Vehicle.m_passengerCount++;

			if (IsDriverSeat)
			{
				Vehicle.Charmer = passenger;
				passenger.Charm = Vehicle;
				Vehicle.UnitFlags |= UnitFlags.Possessed;
			}

			if (passenger is Character)
			{
				var chr = (Character)passenger;
				var pos = Vehicle.Position;

				VehicleHandler.Send_SMSG_ON_CANCEL_EXPECTED_RIDE_VEHICLE_AURA(chr);
				VehicleHandler.SendBreakTarget(chr, Vehicle);

                chr.IncMechanicCount(SpellMechanic.Rooted);
				MovementHandler.SendEnterTransport(chr);
				MiscHandler.SendCancelAutoRepeat(chr, Vehicle);
				MovementHandler.SendMoveToPacket(Vehicle, ref pos, 0, 0, MonsterMoveFlags.Walk);
				PetHandler.SendVehicleSpells(chr, Vehicle);

				chr.SetMover(Vehicle, IsDriverSeat);
				chr.FarSight = Vehicle.EntityId;
			}

			// TODO: Character is now inside the Vehicle but cant move
		}

		/// <summary>
		/// Remove Passenger
		/// </summary>
		public void ClearSeat()
		{
			if (m_passenger == null)
			{
				return;
			}

			if (IsDriverSeat)
			{
				Vehicle.Charmer = null;
				m_passenger.Charm = null;
			}

			Vehicle.m_passengerCount--;
            if (m_passenger.MovementFlags.HasFlag(MovementFlags.Flying))
            {
                var cast = Vehicle.SpellCast;
                    if(cast != null)
                        cast.Trigger(SpellId.EffectParachute);
            }

		    m_passenger.MovementFlags &= ~MovementFlags.OnTransport;
			m_passenger.Auras.RemoveFirstVisibleAura(aura => aura.Spell.IsVehicle);

			if (m_passenger is Character)
			{
				var chr = (Character)m_passenger;
				VehicleHandler.Send_SMSG_ON_CANCEL_EXPECTED_RIDE_VEHICLE_AURA(chr);
                //SendTeleportAck
                MovementHandler.SendMoved(chr);
				MiscHandler.SendCancelAutoRepeat(chr, Vehicle);
				chr.ResetMover();
				chr.FarSight = EntityId.Zero;

				//MovementHandler.SendEnterTransport(chr);
			}
            m_passenger.m_vehicleSeat = null;
		    MovementHandler.SendHeartbeat(m_passenger, m_passenger.Position, m_passenger.Orientation);
            m_passenger.DecMechanicCount(SpellMechanic.Rooted);
			m_passenger = null;
		}
	}
}