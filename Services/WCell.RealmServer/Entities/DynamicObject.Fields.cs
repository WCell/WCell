/*************************************************************************
 *
 *   file		: DynamicObject.Fields.cs
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

using WCell.Constants.Spells;
using WCell.Constants.Updates;

namespace WCell.RealmServer.Entities
{
    public partial class DynamicObject
	{

		public SpellId SpellId
		{
			get { return (SpellId)GetUInt32(DynamicObjectFields.SPELLID); }
			internal set { SetUInt32(DynamicObjectFields.SPELLID, (uint)value); }
		}

        #region DYNAMICOBJET_BYTES

		protected internal uint Bytes
        {
            get { return GetUInt32(DynamicObjectFields.BYTES); }
            internal set { SetUInt32(DynamicObjectFields.BYTES, value); }
        }

        #endregion

        protected internal float Radius
        {
            get { return GetFloat(DynamicObjectFields.RADIUS); }
            internal set { SetFloat(DynamicObjectFields.RADIUS, value); }
        }

        public uint CastTime
        {
            get { return GetUInt32(DynamicObjectFields.CASTTIME); }
            set { SetUInt32(DynamicObjectFields.CASTTIME, value); }
        }



		public override ObjectTypeCustom CustomType
		{
			get
			{
				return ObjectTypeCustom.Object | ObjectTypeCustom.DynamicObject;
			}
		}
    }
}