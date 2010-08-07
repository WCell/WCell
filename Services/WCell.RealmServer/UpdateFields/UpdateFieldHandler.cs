/*************************************************************************
 *
 *   file		: VisibilityManager.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-05-07 20:33:32 +0800 (Wed, 07 May 2008) $
 *   last author	: $LastChangedBy: domiii $
 *   revision		: $Rev: 324 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using WCell.Constants;
using WCell.Constants.Updates;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.Util.Variables;
using WCell.RealmServer.NPCs;
using WCell.Constants.NPCs;

namespace WCell.RealmServer.UpdateFields
{
	/// <summary>
	/// Similar to an UpdateMask, it filters out the bits only needed for the player
	/// </summary>
	public static class UpdateFieldHandler
	{
		/// <summary>
		/// Handles writing of Dynamic UpdateFields. Be sure to definitely
		/// *always* write 4 bytes when a Handler is called.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="receiver"></param>
		/// <param name="packet"></param>
		public delegate void DynamicUpdateFieldHandler(ObjectBase obj, Character receiver, UpdatePacket packet);

		[NotVariable]
		public static DynamicUpdateFieldHandler[] DynamicObjectFieldHandlers;

		[NotVariable]
		public static DynamicUpdateFieldHandler[] DynamicItemFieldHandlers;

		[NotVariable]
		public static DynamicUpdateFieldHandler[] DynamicContainerFieldHandlers;

		[NotVariable]
		public static DynamicUpdateFieldHandler[] DynamicDOFieldHandlers;

		[NotVariable]
		public static DynamicUpdateFieldHandler[] DynamicGOHandlers;

		[NotVariable]
		public static DynamicUpdateFieldHandler[] DynamicCorpseHandlers;

		[NotVariable]
		public static DynamicUpdateFieldHandler[] DynamicUnitHandlers;

		[NotVariable]
		public static DynamicUpdateFieldHandler[] DynamicPlayerHandlers;

		[Initialization(InitializationPass.First)]
		public static void Init()
		{
			UpdateFieldMgr.Init();

			DynamicObjectFieldHandlers = new DynamicUpdateFieldHandler[UpdateFieldMgr.Get(ObjectTypeId.Object).TotalLength];
			DynamicItemFieldHandlers = new DynamicUpdateFieldHandler[UpdateFieldMgr.Get(ObjectTypeId.Item).TotalLength];
			DynamicContainerFieldHandlers = new DynamicUpdateFieldHandler[UpdateFieldMgr.Get(ObjectTypeId.Container).TotalLength];
			DynamicDOFieldHandlers = new DynamicUpdateFieldHandler[UpdateFieldMgr.Get(ObjectTypeId.DynamicObject).TotalLength];
			DynamicGOHandlers = new DynamicUpdateFieldHandler[UpdateFieldMgr.Get(ObjectTypeId.GameObject).TotalLength];
			DynamicCorpseHandlers = new DynamicUpdateFieldHandler[UpdateFieldMgr.Get(ObjectTypeId.Corpse).TotalLength];
			DynamicUnitHandlers = new DynamicUpdateFieldHandler[UpdateFieldMgr.Get(ObjectTypeId.Unit).TotalLength];
			DynamicPlayerHandlers = new DynamicUpdateFieldHandler[UpdateFieldMgr.Get(ObjectTypeId.Player).TotalLength];

			InitHandlers();

			Inherit(DynamicItemFieldHandlers, DynamicObjectFieldHandlers);
			Inherit(DynamicContainerFieldHandlers, DynamicItemFieldHandlers);
			Inherit(DynamicDOFieldHandlers, DynamicObjectFieldHandlers);
			Inherit(DynamicGOHandlers, DynamicObjectFieldHandlers);
			Inherit(DynamicCorpseHandlers, DynamicObjectFieldHandlers);
			Inherit(DynamicUnitHandlers, DynamicObjectFieldHandlers);
			Inherit(DynamicPlayerHandlers, DynamicUnitHandlers);
		}

		private static void Inherit(DynamicUpdateFieldHandler[] handlers, DynamicUpdateFieldHandler[] baseHandlers)
		{
			Array.Copy(baseHandlers, 0, handlers, 0, baseHandlers.Length);
		}

		private static void InitHandlers()
		{
			DynamicGOHandlers[(int)GameObjectFields.DYNAMIC] = WriteGODynamic;
			DynamicCorpseHandlers[(int)CorpseFields.DYNAMIC_FLAGS] = WriteCorpseDynFlags;
			DynamicUnitHandlers[(int)UnitFields.NPC_FLAGS] = WriteNPCFlags;
			DynamicUnitHandlers[(int)UnitFields.DYNAMIC_FLAGS] = WriteUnitDynFlags;
		}

		private static void WriteNPCFlags(ObjectBase obj, Character chr, UpdatePacket packet)
		{
			var flags = (NPCFlags)obj.GetUInt32(UnitFields.NPC_FLAGS);
			if (obj is NPC)
			{
				var npc = (NPC)obj;
				if (npc.IsTrainer && !npc.TrainerEntry.CanTrain(chr))
				{
					// Cannot talk to this Guy
					flags = 0;
				}
			}
			packet.Write((uint)flags);
		}

		private static void WriteGODynamic(ObjectBase obj, Character receiver, UpdatePacket packet)
		{
			packet.Write(obj.GetUInt32(GameObjectFields.DYNAMIC));
		}

		private static void WriteUnitDynFlags(ObjectBase obj, Character receiver, UpdatePacket packet)
		{
			var unit = (Unit)obj;
			//var flags = UnitDynamicFlags.None;
			var flags = unit.DynamicFlags;

			var loot = obj.Loot;
			if (loot != null && receiver.LooterEntry.MayLoot(loot) && !unit.IsAlive)
			{
				flags |= UnitDynamicFlags.Lootable;
			}
			else
			{
				var firstAttacker = unit.FirstAttacker;
				if (firstAttacker != null)
				{
					if ((firstAttacker == receiver ||
						 firstAttacker.IsAlliedWith(receiver)) &&
						unit.IsAlive)
					{
						flags |= UnitDynamicFlags.TaggedByMe;
					}
					else
					{
						flags |= UnitDynamicFlags.TaggedByOther;
					}
				}
			}

			// TODO: TrackUnit, SpecialInfo
			packet.Write((uint)flags);
		}


		private static void WriteCorpseDynFlags(ObjectBase obj, Character receiver, UpdatePacket packet)
		{
			if (((Corpse)obj).Owner == receiver)
			{
				packet.Write((uint)CorpseDynamicFlags.PlayerLootable);
			}
			else
			{
				packet.Write((uint)CorpseDynamicFlags.None);
			}
		}
	}
}