/*************************************************************************
 *
 *   file		: ObjectBase.Update.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-14 13:00:53 +0100 (to, 14 jan 2010) $
 
 *   revision		: $Rev: 1192 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.UpdateFields;

namespace WCell.RealmServer.Entities
{
	public partial class ObjectBase
	{
		protected abstract UpdateType GetCreationUpdateType(UpdateFieldFlags relation);
		

		protected internal bool m_requiresUpdate;

		/// <summary> 
		/// This is a reference to <see cref="m_privateUpdateMask"/> if there are no private values in this object
		/// else its handled seperately
		/// </summary>
		protected internal UpdateMask m_publicUpdateMask;
		protected UpdateMask m_privateUpdateMask;
		protected internal int m_highestUsedUpdateIndex;

		public bool RequiresUpdate
		{
			get { return m_requiresUpdate; }
		}

		public abstract UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicUpdateFieldHandlers
		{
			get;
		}

		internal void ResetUpdateInfo()
		{
			m_privateUpdateMask.Clear();
			if (m_privateUpdateMask != m_publicUpdateMask)
			{
				m_publicUpdateMask.Clear();
			}

			//m_highestUsedUpdateIndex = 0;
			m_requiresUpdate = false;
		}

		/// <summary>
		/// Whether the given field is public
		/// </summary>
		/// <param name="fieldIndex"></param>
		/// <returns></returns>
		public bool IsUpdateFieldPublic(int fieldIndex)
		{
		    return _UpdateFieldInfos.FieldFlags[fieldIndex].HasAnyFlag(UpdateFieldFlags.Public | UpdateFieldFlags.Dynamic);
		}

		/// <summary>
		/// Whether this Object has any private Update fields
		/// </summary>
		/// <returns></returns>
		private bool HasPrivateUpdateFields
		{
			get { return _UpdateFieldInfos.HasPrivateFields; }
		}

		public virtual UpdateFieldFlags GetUpdateFieldVisibilityFor(Character chr)
		{
			return UpdateFieldFlags.Public;
		}

		public abstract void RequestUpdate();
        

		#region Write Updates
		protected internal virtual void WriteObjectCreationUpdate(Character chr)
		{
			var relation = GetUpdateFieldVisibilityFor(chr) | UpdateFieldFlags.Dynamic;
			chr.m_updatePacket.Write((byte)GetCreationUpdateType(relation));
			EntityId.WritePacked(chr.m_updatePacket);
			chr.m_updatePacket.Write((byte)ObjectTypeId);

		    var updateFlags = UpdateFlags;
            if (chr == this)
            {
                updateFlags |= UpdateFlags.Self;
            }

		    var packet = chr.m_updatePacket;

		    packet.Write((ushort) updateFlags);

            /* In 3.1 the update packet changes slightly
             * the UpdateFlags are now 2bytes wide, because of the addition of flags 0x100 and 0x200
             * 0x100 - Stationary object on transport. Mututally exclusive with 0x20 and 0x40. Order checked -> 0x20 > 0x100 > 0x40
             *       - Contents: PackedGuid transportGuid, Vector3 position, Vector3 transportPosition, float facing, float transportFacing
             *       
             * 0x200 - Unknown use
             *       - Contents: int64 - old rotation field from GameObjects
             */

            if (updateFlags.HasAnyFlag(UpdateFlags.Living | UpdateFlags.StationaryObject | UpdateFlags.StationaryObjectOnTransport))
            {
                WriteMovementUpdate(packet, relation);
            }
            if (updateFlags.HasFlag(UpdateFlags.Flag_0x8))
            {
                WriteUpdateFlag_0x8(packet, relation);
            }
            if (updateFlags.HasFlag(UpdateFlags.Flag_0x10))
            {
                WriteUpdateFlag_0x10(packet, relation);
            }

		    WriteTypeSpecificMovementUpdate(packet, relation, updateFlags);

			WriteUpdateValues(true, chr, relation);

			chr.UpdateCount++;
		}

        protected virtual void WriteUpdateFlag_0x8(PrimitiveWriter writer, UpdateFieldFlags relation)
        {
            writer.Write(EntityId.LowRaw);
        }

        protected virtual void WriteUpdateFlag_0x10(PrimitiveWriter writer, UpdateFieldFlags relation)
        {
            writer.Write(1);
        }

        protected virtual void WriteTypeSpecificMovementUpdate(PrimitiveWriter writer, UpdateFieldFlags relation, UpdateFlags updateFlags)
        {
        }

        /// <summary>
        /// Writes the major portion of the create block.
        /// This handles flags 0x20, 0x40, and 0x100, they are exclusive to each other
        /// The content depends on the object's type
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="relation"></param>
        protected virtual void WriteMovementUpdate(PrimitiveWriter writer, UpdateFieldFlags relation)
        {
        }

		public void WriteOutOfRangeEntitiesUpdate(Character receiver, HashSet<WorldObject> worldObjects)
		{
			receiver.m_updatePacket.Write((byte) UpdateType.OutOfRange);
			receiver.m_updatePacket.Write(worldObjects.Count);
			foreach (var worldObject in worldObjects)
			{
				worldObject.EntityId.WritePacked(receiver.m_updatePacket);
			}
			receiver.UpdateCount++;
		}

		public void WriteObjectValueUpdate(Character receiver)
		{
			// TODO: Find a better way to keep track of changed Dynamic UpdateFields
			var relation = GetUpdateFieldVisibilityFor(receiver) | UpdateFieldFlags.Dynamic;

			receiver.m_updatePacket.Write((byte)UpdateType.Values);
			EntityId.WritePacked(receiver.m_updatePacket);
			WriteUpdateValues(false, receiver, relation);

			receiver.UpdateCount++;
		}

		protected void WriteUpdateValues(bool forCreation, Character receiver, UpdateFieldFlags relation)
		{
			UpdateMask mask;
			if (forCreation)
			{
				// completely new
				var pos = receiver.m_updatePacket.Position;
				mask = new UpdateMask(m_highestUsedUpdateIndex);
				receiver.m_updatePacket.Position = pos + 1 + (4 * mask.MaxBlockCount);	// skip over the index block
				for (var i = 0; i <= m_highestUsedUpdateIndex; i++)
				{
					var flags = _UpdateFieldInfos.FieldFlags[i];
					if (flags.HasAnyFlag(relation) && m_updateValues[i].UInt32 != 0)
					{
						mask.SetBit(i);
						WriteUpdateValue(receiver.m_updatePacket, receiver, i);
					}
				}
				var newPos = receiver.m_updatePacket.Position;
				receiver.m_updatePacket.Position = pos;
				mask.WriteFull(receiver.m_updatePacket);								// write the full index block
				receiver.m_updatePacket.Position = newPos;
				return;
				//WriteUpdateValues(receiver, relation, 0, _UpdateFieldInfos.Fields.Length, true);
			}

			if (relation.HasAnyFlag(UpdateFieldFlags.Private))
		    {
		        // Private
		        mask = m_privateUpdateMask;
		    }
		    else if (relation.HasAnyFlag(UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.GroupOnly))
		    {
		        // Group or Owner
		        var pos = receiver.m_updatePacket.Position;
		        mask = new UpdateMask(m_privateUpdateMask.m_highestIndex);
		        receiver.m_updatePacket.Position = pos + 1 + (4 * mask.MaxBlockCount);	// skip over the index block
		        for (var i = m_privateUpdateMask.m_lowestIndex; i <= m_privateUpdateMask.m_highestIndex; i++)
		        {
		            var flags = _UpdateFieldInfos.FieldFlags[i];
					if (flags.HasAnyFlag(relation) &&
						(!flags.HasAnyFlag(UpdateFieldFlags.Public) || m_publicUpdateMask.GetBit(i)))
                    {
                        mask.SetBit(i);
                        WriteUpdateValue(receiver.m_updatePacket, receiver, i);
                    }
		        }
		        var newPos = receiver.m_updatePacket.Position;
		        receiver.m_updatePacket.Position = pos;
		        mask.WriteFull(receiver.m_updatePacket);								// write the full index block
		        receiver.m_updatePacket.Position = newPos;
		        //WriteUpdateValues(receiver, relation, m_privateUpdateMask.m_lowestIndex, m_privateUpdateMask.m_highestIndex, false);
		        return;
		    }
		    else
		    {
		        // Public
		        mask = m_publicUpdateMask;
		    }

		    mask.WriteTo(receiver.m_updatePacket);

		    for (var i = mask.m_lowestIndex; i <= mask.m_highestIndex; i++)
		    {
		        if (mask.GetBit(i))
		        {
		            WriteUpdateValue(receiver.m_updatePacket, receiver, i);
		        }
		    }
		}

		protected void WriteUpdateValue(UpdatePacket packet, Character receiver, int index)
		{
			if (_UpdateFieldInfos.FieldFlags[index].HasAnyFlag(UpdateFieldFlags.Dynamic))
			{
				DynamicUpdateFieldHandlers[index](this, receiver, packet);
			}
			else
			{
				packet.Write(m_updateValues[index].UInt32);
			}
		}

		public void SendSpontaneousUpdate(Character receiver, params UpdateFieldId[] indices)
		{
			SendSpontaneousUpdate(receiver, true, indices);
		}

		public void SendSpontaneousUpdate(Character receiver, bool visible, params UpdateFieldId[] indices)
		{
			var highestIndex = 0;
			foreach (var index in indices)
			{
				if (index.RawId > highestIndex)
				{
					highestIndex = index.RawId;
				}
			}

			var mask = new UpdateMask(highestIndex);
			using (var packet = new UpdatePacket(1024))
			{
				packet.Position = 4;						// jump over header
				packet.Write(1);							// Update Count
				packet.Write((byte)UpdateType.Values);
				EntityId.WritePacked(packet);
				WriteSpontaneousUpdate(mask, packet, receiver, indices, visible);

				receiver.Send(packet);
			}
		}

		protected void WriteSpontaneousUpdate(UpdateMask mask, UpdatePacket packet, Character receiver, UpdateFieldId[] indices, bool visible)
		{
			// create mask
            for (var i = 0; i < indices.Length; i++)
            {
            	var index = indices[i].RawId;
            	var field = UpdateFieldMgr.Get(ObjectTypeId).Fields[index];
				for (var j = 0; j < field.Size; j++)
				{
					mask.SetBit(index + j);
				}
            }

			// write mask
			mask.WriteTo(packet);

			// write values
			for (var i = mask.m_lowestIndex; i <= mask.m_highestIndex; i++)
			{
				if (mask.GetBit(i))
				{
					if (visible)
					{
						WriteUpdateValue(packet, receiver, i);
					}
					else
					{
						packet.Write(0);
					}
				}
			}
		}

		#endregion

		public RealmPacketOut CreateDestroyPacket()
		{
			var packet = new RealmPacketOut(RealmServerOpCode.SMSG_DESTROY_OBJECT, 9);
			packet.Write(EntityId);

			packet.Write((byte)0); // NEW 3.0.2

			return packet;
		}

		public void SendDestroyToPlayer(Character c)
		{
			using (var packet = CreateDestroyPacket())
			{
				c.Client.Send(packet);
			}
		}

		public void SendOutOfRangeUpdate(Character receiver, HashSet<WorldObject> worldObjects)
		{
			using (var packet = new UpdatePacket(1024))
			{
				packet.Position = 4;						// jump over header
				packet.Write(1);							// Update Count
				packet.Write((byte)UpdateType.OutOfRange);
				packet.Write(worldObjects.Count);
				foreach (var worldObject in worldObjects)
				{
					worldObject.EntityId.WritePacked(packet);
				}

				receiver.Send(packet);
			}
		}
		
		// TODO: Improve (a lot)

		#region Spontaneous UpdateBlock Creation

		protected UpdatePacket GetFieldUpdatePacket(UpdateFieldId field, uint value)
		{
			var blocks = (field.RawId >> 5) + 1;
			var emptyBlockSize = (blocks - 1) * 4;

			//UpdatePacket packet = new UpdatePacket(BufferManager.Small.CheckOut());
			var packet = new UpdatePacket { Position = 4 };

			packet.Write(1); // Update Count
			packet.Write((byte)UpdateType.Values);

			EntityId.WritePacked(packet);

			packet.Write((byte)blocks);

			//packet.TotalLength += emptyBlockSize;
			packet.Zero(emptyBlockSize);

			packet.Write(1 << (field.RawId & 31));
			packet.Write(value);

			return packet;
		}

		protected UpdatePacket GetFieldUpdatePacket(UpdateFieldId field, byte[] value)
		{
			var blocks = (field.RawId >> 5) + 1;
			var emptyBlockSize = (blocks - 1) * 4;

			//UpdatePacket packet = new UpdatePacket(BufferManager.Small.CheckOut());
			var packet = new UpdatePacket { Position = 4 };

			packet.Write(1); // Update Count
			packet.Write((byte)UpdateType.Values);

			EntityId.WritePacked(packet);

			packet.Write((byte)blocks);

			//packet.TotalLength += emptyBlockSize;
			packet.Zero(emptyBlockSize);

			packet.Write(1 << (field.RawId & 31));
			packet.Write(value);

			return packet;
		}

		protected UpdatePacket GetFieldUpdatePacket(UpdateFieldId field, EntityId value)
		{
			//UpdatePacket packet = new UpdatePacket(BufferManager.Small.CheckOut());
			var packet = new UpdatePacket(128) { Position = 4 };

			packet.Write(1); // Update Count
			packet.Write((byte)UpdateType.Values);

			EntityId.WritePacked(packet);

			var blocks = (byte)((field.RawId + 1) / 32 + 2);
			packet.Write(blocks);

			if (blocks > 1)
			{
				packet.Zero((blocks - 2) * 4);

				var updateBlocks = new int[blocks];

				updateBlocks[field.RawId << 5] = (1 << (field.RawId & 31));
				updateBlocks[field.RawId + 1 << 5] = (1 << (field.RawId + 1 & 31));

				packet.Write(updateBlocks[0]);
				packet.Write(updateBlocks[1]);
			}
			else
			{
				packet.Zero((blocks - 1) * 4);
				packet.Write((1 << (field.RawId & 31) | 1 << (field.RawId + 1 & 31)));
			}

			packet.Write(value);

			return packet;
		}

		public void PushFieldUpdateToPlayer(Character character, UpdateFieldId field, int value)
		{
			using (var packet = GetFieldUpdatePacket(field, (uint)value))
			{
				SendUpdatePacket(character, packet);
			}
		}

		public void PushFieldUpdateToPlayer(Character character, UpdateFieldId field, uint value)
		{
			using (var packet = GetFieldUpdatePacket(field, value))
			{
				SendUpdatePacket(character, packet);
			}
		}

		public void PushFieldUpdateToPlayer(Character character, UpdateFieldId field, byte[] value)
		{
			using (var packet = GetFieldUpdatePacket(field, value))
			{
				SendUpdatePacket(character, packet);
			}
		}

	    protected static void SendUpdatePacket(Character character, UpdatePacket packet)
		{
			packet.SendTo(character.Client);
		}

	    #endregion
	}
}