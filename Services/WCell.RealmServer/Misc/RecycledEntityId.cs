/*************************************************************************
 *
 *   file		: RecycledEntityId.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-01-31 12:35:36 +0100 (to, 31 jan 2008) $
 *   last author	: $LastChangedBy: tobz $
 *   revision		: $Rev: 87 $
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
using WCell.RealmServer.Database;
using NLog;
using WCell.RealmServer.Localization;

namespace WCell.RealmServer.Database
{
	/// <summary>
	/// Represents a recycled entity ID.
	/// </summary>
	public partial class RecycledEntityId
	{
		#region Compiled Queries

		public static Func<RealmServerDataContext, int, IQueryable<RecycledEntityId>> RecycledIdsByType =
			CompiledQuery.Compile(
				(RealmServerDataContext rdb, int id) =>
				from rid in rdb.RecycledEntityIds
				where rid.EntityType == id
				select rid
			);

		public static Func<RealmServerDataContext, int, int, IQueryable<RecycledEntityId>> RecycledIdByLowerAndType =
			CompiledQuery.Compile(
				(RealmServerDataContext rdb, int lowerId, int idType) =>
				from rid in rdb.RecycledEntityIds
				where rid.EntityType == idType && rid.EntityId == lowerId
				select rid
			);

		#endregion

		#region Fields

		private static SpinWaitLock m_idLock = new SpinWaitLock();
        private static Logger s_log = LogManager.GetCurrentClassLogger();

		#endregion

		#region Methods

		/// <summary>
        /// Tries to get the first available recycled ID.
        /// </summary>
        /// <param name="entityIdType">the type of the entity id</param>
        /// <returns>a non-zero ID if one was available; 0 otheriwse</returns>
		/// <remarks>If an available ID is found, it'll be removed from the database.</remarks>
        public static uint TryGetLowerEntityId(EntityIdType entityIdType)
        {
            // Lock so someone else doesn't grab the same row
            m_idLock.Enter();

			try
			{
                RecycledEntityId eid = GetFirstRecycledId(entityIdType);

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
				m_idLock.Exit();
			}
		}

		/// <summary>
		/// Recycles an entity ID of the given type.
		/// </summary>
		/// <param name="lowerEntityId">the lower entity id</param>
		/// <param name="entityIdType">the type of the entity id</param>
		public static bool RecycleLowerEntityId(uint lowerEntityId, EntityIdType idType)
		{
			if (DoesIdExist(lowerEntityId, idType))
			{
				// TODO: What should we do if it already exists? This is are a serious bug.
				s_log.Debug(Resources.AlreadyRecycledEntityId, lowerEntityId, idType.ToString());

                return false;
			}

			RecycledEntityId eid = new RecycledEntityId();
            eid.RecycledEntityIdGuid = Guid.NewGuid();
			eid.EntityId = (long)lowerEntityId;
			eid.EntityType = (int)idType;

            AddRecycledId(eid);

            return true;
		}

		/// <summary>
		/// Gets the first available ID of the given type.
		/// </summary>
		/// <param name="type">the type of entity ID to get</param>
		/// <returns>a <see cref="RecycledEntityId" /> object representing the ID; null if no ID was available</returns>
        public static RecycledEntityId GetFirstRecycledId(EntityIdType type)
        {
            using (var db = RealmServerDataContext.GetContext())
            {
                IQueryable<RecycledEntityId> ids = RecycledIdsByType(db, (int)type);

                return ids.FirstOrDefault();
            }
        }

		/// <summary>
		/// Checks if the recycled ID already exists in the database.
		/// </summary>
		/// <param name="lowerId">the ID to check for</param>
		/// <param name="type">the entity ID type to check for</param>
		/// <returns>true if the ID has already been recycled; false if not</returns>
        public static bool DoesIdExist(uint lowerId, EntityIdType type)
        {
            using (var db = RealmServerDataContext.GetContext())
            {
                return RecycledIdByLowerAndType(db, (int)lowerId, (int)type).Count() > 0;
            }
        }

		/// <summary>
		/// Adds a new recycled ID to the database.
		/// </summary>
		/// <param name="id">the ID to add</param>
        public static void AddRecycledId(RecycledEntityId id)
        {
            using (var db = RealmServerDataContext.GetContext())
            {
                db.RecycledEntityIds.InsertOnSubmit(id);
                db.SubmitChanges();
            }
        }

		/// <summary>
		/// Removes a recycled ID from the database.
		/// </summary>
		/// <param name="id">the ID to remove</param>
        public static void RemoveRecycledId(RecycledEntityId id)
        {
            using (var db = RealmServerDataContext.GetContext())
            {
                db.RecycledEntityIds.Attach(id);
                db.RecycledEntityIds.DeleteOnSubmit(id);
                db.SubmitChanges();
            }
        }

        #endregion
    }
}
