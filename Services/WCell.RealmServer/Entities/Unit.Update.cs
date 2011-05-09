/*************************************************************************
 *
 *   file		: Unit.Update.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-04-15 16:51:00 +0200 (to, 15 apr 2010) $
 *   last author	: $LastChangedBy: xtzgzorex $
 *   revision		: $Rev: 1276 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;
using WCell.Constants.Updates;
using WCell.Core.Network;
using WCell.RealmServer.UpdateFields;
using WCell.Util;
using WCell.Constants.Spells;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Entities
{
	public partial class Unit
	{
		#region Update Field Management
		public override UpdateFlags UpdateFlags
		{
			get { return UpdateFlags.StationaryObject | UpdateFlags.Living; }
		}

		public override ObjectTypeId ObjectTypeId
		{
			get { return ObjectTypeId.Unit; }
		}

		public override UpdateFieldFlags GetUpdateFieldVisibilityFor(Character chr)
		{
			if (chr == m_master)
			{
				return UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.Public;
			}
			return UpdateFieldFlags.Public;
		}

		public override UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicUpdateFieldHandlers
		{
			get { return UpdateFieldHandler.DynamicUnitHandlers; }
		}
		#endregion

		#region Movement
		public virtual WorldObject Mover
		{
			get { return this; }
		}

		protected override void WriteMovementUpdate(PrimitiveWriter packet, UpdateFieldFlags relation)
		{
			WriteMovementPacketInfo(packet);

			#region Speed Block

			packet.Write(WalkSpeed);
			packet.Write(RunSpeed);
			packet.Write(RunBackSpeed);
			packet.Write(SwimSpeed);
			packet.Write(SwimBackSpeed);
			packet.Write(FlightSpeed);
			packet.Write(FlightBackSpeed);
			packet.Write(TurnSpeed);
			packet.Write(PitchRate);

			#endregion

			#region Spline Info
            if (MovementFlags.HasFlag(MovementFlags.SplinePath))
			{
				// TODO: Write spline flags
				//var splineFlags = SplineFlags.None;
			}

			#endregion
		}

		protected override void WriteTypeSpecificMovementUpdate(PrimitiveWriter writer, UpdateFieldFlags relation, UpdateFlags updateFlags)
		{
			// Probably specific to Unit
            if (updateFlags.HasFlag(UpdateFlags.AttackingTarget))
			{
				writer.Write((byte)0); // pguid
			}
		}

        protected override void WriteUpdateFlag_0x10(PrimitiveWriter writer, UpdateFieldFlags relation)
        {
            writer.Write(150754760); // TODO - wtf?
            //base.WriteUpdateFlag_0x10(writer, relation);
        }

		/// <summary>
		/// Writes the data shared in movement packets and the create block of the update packet
		/// This is used in
		/// <list type="String">
		/// SMSG_UPDATE_OBJECT
		/// MSG_MOVE_*
		/// MSG_MOVE_SET_*_SPEED
		/// </list>
		/// </summary>
		/// <param name="packet"></param>
		public void WriteMovementPacketInfo(PrimitiveWriter packet)
		{
			WriteMovementPacketInfo(packet, ref m_position, m_orientation);
		}

		/// <summary>
		/// Writes the data shared in movement packets and the create block of the update packet
		/// This is used in
		/// <list type="String">
        /// SMSG_UPDATE_OBJECT
        /// MSG_MOVE_*
        /// MSG_MOVE_SET_*_SPEED
		/// </list>
		/// </summary>
		/// <param name="packet"></param>
		public void WriteMovementPacketInfo(PrimitiveWriter packet, ref Vector3 pos, float orientation)
		{
			var moveFlags = MovementFlags;
			var moveFlags2 = MovementFlags2;

			if (moveFlags.HasAnyFlag(MovementFlags.OnTransport) && TransportInfo == null)
            {
                // should never happen
                moveFlags ^= MovementFlags.OnTransport;
            }

		    packet.Write((uint)moveFlags);
			packet.Write((ushort)moveFlags2);
			packet.Write(Utility.GetSystemTime());
			packet.Write(pos.X);
			packet.Write(pos.Y);
			packet.Write(pos.Z);
			packet.Write(orientation);

			if (moveFlags.HasAnyFlag(MovementFlags.OnTransport))
			{
// ReSharper disable PossibleNullReferenceException
				TransportInfo.EntityId.WritePacked(packet);
// ReSharper restore PossibleNullReferenceException
				packet.Write(TransportPosition.X);
				packet.Write(TransportPosition.Y);
				packet.Write(TransportPosition.Z);
				packet.Write(TransportOrientation);
				packet.Write(TransportTime);
				packet.Write(TransportSeat);

                if (moveFlags2.HasFlag(MovementFlags2.MoveFlag2_10_0x400))
                {
                    packet.Write(0);
                }
			}

			if (moveFlags.HasAnyFlag(MovementFlags.Swimming | MovementFlags.Flying) ||
                moveFlags2.HasFlag(MovementFlags2.AlwaysAllowPitching))
			{
				packet.Write(PitchRate);
			}

            if (moveFlags2.HasFlag(MovementFlags2.InterpolatedTurning))
            {
                packet.Write(0); // air time
                // constant, but different when jumping in water and on land?                
                packet.Write(0f);

                if (moveFlags.HasFlag(MovementFlags.Falling))
                {
                    // yet somewhat unknown values (Client sends them)
                    packet.Write(8f);
                    packet.Write(0.2f);
                    packet.Write(1f);

                }
            }

			if (moveFlags.HasAnyFlag(MovementFlags.SplineElevation))
			{
				packet.Write(0.0f);
			}
		}

		public void WriteTeleportPacketInfo(RealmPacketOut packet, int param)
		{
			EntityId.WritePacked(packet);

			packet.Write(param);
			WriteMovementPacketInfo(packet);

		}
		#endregion

		#region Update()

		public override void Update(int dt)
		{
			base.Update(dt);

			Regenerate(dt);
			if (m_brain != null)
			{
				m_brain.Update(dt);
			}

			m_attackTimer.Update(dt);

			if (m_TaxiMovementTimer != null)
			{
				m_TaxiMovementTimer.Update(dt);
			}
			
			foreach (var aura in m_auras)
			{
				aura.Update(dt);
			}
		}

		//public override UpdatePriority UpdatePriority
		//{
		//    get
		//    {
		//        return m_brain != null ? m_brain.Priority : base.UpdatePriority;
		//    }
		//}
		#endregion
	}
}