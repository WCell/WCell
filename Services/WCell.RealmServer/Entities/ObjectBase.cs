/*************************************************************************
 *
 *   file		: ObjectBase.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-14 13:00:53 +0100 (to, 14 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1192 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using WCell.Constants.Updates;
using WCell.RealmServer.Looting;
using WCell.RealmServer.UpdateFields;

namespace WCell.RealmServer.Entities
{
    /// <summary>
    /// The base class for all in-game Objects
    /// </summary>
	public abstract partial class ObjectBase : IDisposable, ILootable, IEntity
    {
		protected readonly CompoundType[] m_updateValues;

        protected ObjectBase()
		{
            int updateFieldInfoLen = _UpdateFieldInfos.Fields.Length;

			m_privateUpdateMask = new UpdateMask(updateFieldInfoLen);

			// we only need a distinction between private and public if we have any non-public update fields
			if (HasPrivateUpdateFields)
			{
				m_publicUpdateMask = new UpdateMask(updateFieldInfoLen);
			}
			else
			{
				m_publicUpdateMask = m_privateUpdateMask;
			}

			m_updateValues = new CompoundType[updateFieldInfoLen];

            Type = ObjectTypes.Object;
            SetFloat(ObjectFields.SCALE_X, 1.0f);
		}

        protected abstract UpdateFieldCollection _UpdateFieldInfos
		{
			get;
		}

		#region Properties

		public abstract UpdateFlags UpdateFlags
		{
			get;
		}

		/// <summary>
		/// The type of this object (player, corpse, item, etc)
		/// </summary>
		public abstract ObjectTypeId ObjectTypeId
		{
			get;
		}

		public CompoundType[] UpdateValues
		{
			get { return m_updateValues; }
		}

		public UpdateMask UpdateMask
		{
			get { return m_privateUpdateMask; }
		}
		#endregion

		public abstract bool IsInWorld
		{
			get;
		}

        public abstract void Dispose(bool disposing);

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

		public bool CheckObjType(ObjectTypes type)
		{
			return type == ObjectTypes.None || Type.HasAnyFlag(type);
		}

    	public virtual bool UseGroupLoot
    	{
			get { return false; }
    	}

    	/// <summary>
		/// Called whenever everything has been looted off this object.
		/// </summary>
		public virtual void OnFinishedLooting()
		{
		}

    	public virtual UpdatePriority UpdatePriority
    	{
			get { return UpdatePriority.LowPriority; }
    	}

        public override string ToString()
        {
            return GetType().Name + " (ID: " + EntityId + ")";
        }
    }
}