/*************************************************************************
 *
 *   file		: Corpse.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-09-16 17:25:58 +0200 (on, 16 sep 2009) $

 *   revision		: $Rev: 1102 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Threading;
using NLog;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.Items;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Items;
using WCell.RealmServer.Network;
using WCell.RealmServer.UpdateFields;
using WCell.Util.Graphics;
using WCell.Util.Variables;

namespace WCell.RealmServer.Entities
{
	public partial class Corpse : WorldObject
	{
		public override UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicUpdateFieldHandlers
		{
			get { return UpdateFieldHandler.DynamicCorpseHandlers; }
		}

		private static float s_reclaimRadius;
		internal static float ReclaimRadiusSq;
		private static float s_corpseVisRange;
		internal static float GhostVisibilityRadiusSq;

		/// <summary>
		/// The radius (in yards) in which a Character has to be in order to reclaim his/her corpse (Default: 40)
		/// </summary>
		public static float MinReclaimRadius
		{
			get
			{
				return s_reclaimRadius;
			}
			set
			{
				s_reclaimRadius = value;
				ReclaimRadiusSq = s_reclaimRadius * s_reclaimRadius;
			}
		}

		/// <summary>
		/// The radius (in yards) around a corpse in which the dead owner can see
		/// living Units
		/// </summary>
		public static float GhostVisibilityRadius
		{
			get
			{
				return s_corpseVisRange;
			}
			set
			{
				s_corpseVisRange = value;
				GhostVisibilityRadiusSq = s_corpseVisRange * s_corpseVisRange;
			}
		}


		static Corpse()
		{
			// default radius
			MinReclaimRadius = 40f;
			GhostVisibilityRadius = 60f;
		}

		[Variable("CorpseMinReclaimDelayMillis")]
		/// <summary>
		/// The delay between last res and when the Character is allowed to claim his/her corpse again in millis (Default: 30 sec)
		/// </summary>
		public static int MinReclaimDelay = 30 * 1000;

		[Variable("CorpseAutoReleaseDelayMillis")]
		/// <summary>
		/// The delay between death and when the Character auto-revives in millis (Default: 6 min)
		/// </summary>
		public static int AutoReleaseDelay = 60 * 6 * 1000;

		[Variable("BonesDecayTimeMillis")]
		/// <summary>
		/// Time before bones disappear in seconds (Default: 1 minute)
		/// </summary>
		public static int DecayTimeMillis = 60 * 1000;

		public static readonly UpdateFieldCollection UpdateFieldInfos = UpdateFieldMgr.Get(ObjectTypeId.Corpse);

	    protected override UpdateFieldCollection _UpdateFieldInfos
		{
			get { return UpdateFieldInfos; }
		}

		internal static readonly CompoundType[] EmptyItemFields =
			new CompoundType[(CorpseFields.ITEM_19 - CorpseFields.ITEM) + 1];

		static int lastUID;

        private static readonly Logger log = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Creates a new Corpse for the given owner
		/// </summary>
		/// <param name="owner">The owner of this Corpse</param>
		/// <param name="pos">The position where the Corpse should appear</param>
		/// <param name="orientation">Orientation of the corpse</param>
		/// <param name="displayId">The displayid of the corpse</param>
		/// <param name="face">Face value</param>
		/// <param name="skin">Skin value</param>
		/// <param name="hairStyle">Hairstyle</param>
		/// <param name="hairColor">Haircolor</param>
		/// <param name="facialHair">Facial hair (beard)</param>
		/// <param name="guildId">The guild to which the owner of the corpse belongs</param>
		/// <param name="gender">Gender of the owner</param>
		/// <param name="race">Race of the owner</param>
		/// <param name="flags">Flags (only skeleton or full corpse)</param>
		/// <param name="dynFlags">Dynamic flags (is it lootable?)</param>
		public Corpse(Character owner, Vector3 pos, float orientation, uint displayId,
			byte face,
			byte skin,
			byte hairStyle,
			byte hairColor,
			byte facialHair,
			uint guildId,
			GenderType gender,
			RaceId race,
			CorpseFlags flags,
			CorpseDynamicFlags dynFlags)
		{
			EntityId = EntityId.GetCorpseId((uint)Interlocked.Increment(ref lastUID));
			DisplayId = displayId;
			Owner = owner;
			Type |= ObjectTypes.Corpse;
			ScaleX = 1;

		    m_position = pos;
            m_orientation = orientation;

			Face = face;
			Skin = skin;
			HairStyle = hairStyle;
			HairColor = hairColor;
			FacialHair = facialHair;
			GuildId = guildId;
			Gender = gender;
			Race = race;
			Flags = flags;
			DynamicFlags = dynFlags;
		}

		public override string Name
		{
			get { return m_owner != null ? ("Corpse of " + m_owner) : "Unknown Corpse"; }
			set
			{
				// do nothing
			}
		}

		public override ObjectTypeId ObjectTypeId
		{
			get { return ObjectTypeId.Corpse; }
		}

		public override UpdateFlags UpdateFlags
		{
			get { return UpdateFlags.StationaryObject | UpdateFlags.Flag_0x10 | UpdateFlags.StationaryObjectOnTransport; }
		}

		public override Faction Faction
		{
			get
			{
				return m_owner.Faction;
			}
			set
			{
				throw new Exception("Corpse' faction can't be changed");
			}
		}

		public override FactionId FactionId
		{
			get
			{
				return m_owner.Faction.Id;
			}
			set
			{
				throw new Exception("Corpse' faction can't be changed");
			}
		}

		public override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			m_owner = null;
		}

		protected override void WriteMovementUpdate(PrimitiveWriter packet, UpdateFieldFlags relation)
		{
			if (UpdateFlags.HasAnyFlag(UpdateFlags.StationaryObjectOnTransport))
            {
                // corpses had this, but seemed to just send their own info for both
                EntityId.Zero.WritePacked(packet);
                packet.Write(Position);
                packet.Write(Position); // transport position, but server seemed to send normal position except orientation
                packet.Write(Orientation);
                packet.Write(Orientation);
            }
			else if (UpdateFlags.HasAnyFlag(UpdateFlags.StationaryObject))
            {
                #region UpdateFlag.Flag_0x40 (StationaryObject)

                packet.Write(Position);
                packet.WriteFloat(Orientation);

                #endregion
            }
		}


		internal protected override void OnEnterMap()
		{
		}

		internal protected override void OnLeavingMap()
		{
			base.OnLeavingMap();
		}

		public override void OnFinishedLooting()
		{
			StartDecay();
		}

		/// <summary>
		/// Set the Item at the given slot on this corpse.
		/// </summary>
		public void SetItem(EquipmentSlot slot, ItemTemplate template)
		{
			//var id = (template.DisplayId & 0x00FFFFFF) | (uint)((int)template.InventorySlotType << 24);
			var id = template.DisplayId | (uint)((int)template.InventorySlotType << 24);
			var slotId = (int)CorpseFields.ITEM + (int)slot;
            
            SetUInt32(slotId, id);

			//Array.Copy(characterFields, (int)PlayerFields.VISIBLE_ITEM_1_0,
			//    m_updateValues, (int)CorpseFields.ITEM, EmptyItemFields.Length);

			//if (!m_queuedForUpdate && m_isInWorld)
			//{
			//    RequestUpdate();
			//}
		}

		/// <summary>
		/// Removes the Items from this Corpse
		/// </summary>
		public void RemoveItems()
		{
			Array.Copy(EmptyItemFields, 0,
				m_updateValues, (int)CorpseFields.ITEM, EmptyItemFields.Length);

			if (!m_requiresUpdate && IsInWorld)
			{
				RequestUpdate();
			}
		}

		/// <summary>
		/// Starts the decay-timer
		/// </summary>
		public void StartDecay()
		{
			if (IsInWorld && Flags != CorpseFlags.Bones)
			{
				RemoveItems();
				//Flags = CorpseFlags.Bones;
				DynamicFlags = CorpseDynamicFlags.None;
				m_Map.CallDelayed(DecayTimeMillis, Delete);
			}
			else
			{
				Delete();
			}
		}
	}
}