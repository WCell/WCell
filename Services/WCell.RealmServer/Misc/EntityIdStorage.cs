/*************************************************************************
 *
 *   file		: RecycledEntityId.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-06-12 13:16:40 +0200 (to, 12 jun 2008) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 491 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using Cell.Core;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using WCell.Core;
using WCell.Constants;
using WCell.RealmServer.Database;
using NLog;
using WCell.RealmServer.Localization;
using NHibernate.Expression;
using Castle.ActiveRecord.Queries;
using Castle.ActiveRecord;

namespace WCell.RealmServer.Database
{
	/// <summary>
	/// Represents a recycled entity ID.
	/// </summary>
	public partial class EntityIdStorage
	{
		private static SpinWaitLock s_idLock = new SpinWaitLock();
		private static Logger s_log = LogManager.GetCurrentClassLogger();
		private static uint s_highestPlayerId = 0;
		private static uint s_highestItemId = 0;

		/// <summary>
		/// Tries to get the first available entity ID.
		/// </summary>
		
		/// <returns>a non-zero ID if one was available; 0 otheriwse</returns>
		/// <remarks>If an available ID is found, it will be removed from the database.</remarks>
		public static uint GetLowEntityId(ObjectTypeId type)
		{
			// Lock so someone else doesn't grab the same row
			s_idLock.Enter();

			try
			{
				EntityIdStorage eid = GetFirstRecycledId(type);

				if (eid == null)
				{
					return 0;
				}
				else
				{
					RemoveRecycledId(eid);

					return (uint)eid.EntityId;
				}
			}
			finally
			{
				s_idLock.Exit();
			}
		}

		/// <summary>
		/// Tries to get the first available player entity ID.
		/// </summary>
		
		/// <returns>a non-zero ID if one was available; 0 otheriwse</returns>
		/// <remarks>If an available ID is found, it will be removed from the database.</remarks>
		public static uint GetPlayerEntityId()
		{
			// Lock so someone else doesn't grab the same row
			s_idLock.Enter();

			try
			{
				EntityIdStorage eid = GetFirstRecycledId(ObjectTypeId.Player);

				if (eid == null)
				{
					if (CharacterRecord.Exists())
					{
						if (s_highestPlayerId == 0)
						{
							CharacterRecord highestChr =
								CharacterRecord.FindFirst(new Order("_entityLowId", false));

							s_highestPlayerId = highestChr.EntityLowId;
						}
					}

					return ++s_highestPlayerId;
				}
				else
				{
					RemoveRecycledId(eid);

					return (uint)eid.EntityId;
				}
			}
			catch (Exception ex)
			{
				s_log.ErrorException("couldn't get an entity id", ex);
			}
			finally
			{
				s_idLock.Exit();
			}

			return 0;
		}

		/// <summary>
		/// Tries to get the first available player entity ID.
		/// </summary>
		
		/// <returns>a non-zero ID if one was available; 0 otheriwse</returns>
		/// <remarks>If an available ID is found, it will be removed from the database.</remarks>
		public static uint GetItemEntityId()
		{
			// Lock so someone else doesn't grab the same row
			s_idLock.Enter();

			try
			{
				EntityIdStorage eid = GetFirstRecycledId(ObjectTypeId.Item);

				if (eid == null)
				{
					//if (ItemRecord.Exists())
					if (s_highestItemId == 0)
					{
						//ItemRecord highestItem =
						//	ItemRecord.FindFirst(new Order("_entityId", false));

						//s_highestItemId = highestItem.EntityId;
						s_highestItemId = 1;
					}

					return ++s_highestItemId;
				}
				else
				{
					RemoveRecycledId(eid);

					return (uint)eid.EntityId;
				}
			}
			catch (Exception ex)
			{
				s_log.ErrorException("Could not fetch EntityId", ex);
			}
			finally
			{
				s_idLock.Exit();
			}

			return 0;
		}

		/// <summary>
		/// Recycles an entity ID of the given type.
		/// </summary>
		/// <param name="lowerEntityId">the lower entity id</param>
		
		public static bool RecycleLowerEntityId(uint lowerEntityId, ObjectTypeId idType)
		{
			if (DoesIdExist(lowerEntityId, idType))
			{
				// TODO: What should we do if it already exists? This is are a serious bug.
				s_log.Debug(Resources.AlreadyRecycledEntityId, lowerEntityId.ToString(), idType.ToString());

				return false;
			}

			EntityIdStorage eid = new EntityIdStorage();
			eid.EntityId = lowerEntityId;
			eid.EntityType = idType;

			AddRecycledId(eid);

			return true;
		}

		/// <summary>
		/// Gets the first available ID of the given type.
		/// </summary>
		/// <param name="type">the type of entity ID to get</param>
		/// <returns>a <see cref="EntityIdStorage" /> object representing the ID; null if no ID was available</returns>
		private static EntityIdStorage GetFirstRecycledId(ObjectTypeId type)
		{
			return FindFirst(new Order("EntityId", true), new EqExpression("EntityType", type));
		}

		private static uint GetNextHighestPlayerId()
		{
			ScalarQuery idQuery = new ScalarQuery(typeof(CharacterRecord), QueryLanguage.Hql,
												  "select max(chr.EntityId) from CharacterRecord chr");
			object highestId = ActiveRecordMediator.ExecuteQuery(idQuery);

			return (highestId == null ? 0 : (highestId as CharacterRecord).EntityLowId + 1);
		}

		/// <summary>
		/// Checks if the recycled ID already exists in the database.
		/// </summary>
		/// <param name="lowerId">the ID to check for</param>
		/// <param name="type">the entity ID type to check for</param>
		/// <returns>true if the ID has already been recycled; false if not</returns>
		public static bool DoesIdExist(uint lowerId, ObjectTypeId type)
		{
			return Exists(new EqExpression("EntityId", (long)lowerId), new EqExpression("EntityType", type));
		}

		/// <summary>
		/// Tries to retrieve an entity ID from the database.
		/// </summary>
		/// <param name="lowerId">the ID to check for</param>
		/// <param name="type">the entity ID type to check for</param>
		/// <returns>true if the ID has already been recycled; false if not</returns>
		public static EntityIdStorage GetEntityId(uint lowerId, ObjectTypeId type)
		{
			s_idLock.Enter();

			try
			{
				return FindFirst(new EqExpression("EntityId", (long)lowerId), new EqExpression("EntityType", type));
			}
			finally
			{
				s_idLock.Exit();
			}
		}

		/// <summary>
		/// Adds a new recycled ID to the database.
		/// </summary>
		/// <param name="id">the ID to add</param>
		public static void AddRecycledId(EntityIdStorage id)
		{
			id.SaveCopyAndFlush();
		}

		/// <summary>
		/// Removes a recycled ID from the database.
		/// </summary>
		/// <param name="id">the ID to remove</param>
		public static void RemoveRecycledId(EntityIdStorage id)
		{
			id.DeleteAndFlush();
		}
	}
}
