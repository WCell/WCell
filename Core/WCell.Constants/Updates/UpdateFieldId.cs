/*************************************************************************
 *
 *   file		: UpdateFieldId.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-01-31 19:35:36 +0800 (Thu, 31 Jan 2008) $
 *   last author	: $LastChangedBy: tobz $
 *   revision		: $Rev: 87 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/


namespace WCell.Constants.Updates
{
	public struct UpdateFieldId
	{
		public readonly int RawId;

	    private UpdateFieldId(int rawId)
		{
			RawId = rawId;
		}

		public static implicit operator int(UpdateFieldId field)
		{
			return field.RawId;
		}

	    public static implicit operator UpdateFieldId(ObjectFields val)
		{
			return new UpdateFieldId((int)val);
		}

		public static implicit operator UpdateFieldId(UnitFields val)
		{
			return new UpdateFieldId((int)val);
		}

		public static implicit operator UpdateFieldId(PlayerFields val)
		{
			return new UpdateFieldId((int)val);
		}

		public static implicit operator UpdateFieldId(CorpseFields val)
		{
			return new UpdateFieldId((int)val);
		}

		public static implicit operator UpdateFieldId(ItemFields val)
		{
			return new UpdateFieldId((int)val);
		}

		public static implicit operator UpdateFieldId(ContainerFields val)
		{
			return new UpdateFieldId((int)val);
		}

		public static implicit operator UpdateFieldId(DynamicObjectFields val)
		{
			return new UpdateFieldId((int)val);
		}

		public static implicit operator UpdateFieldId(GameObjectFields val)
		{
			return new UpdateFieldId((int)val);
		}
	}

	public struct ExtendedUpdateFieldId
	{
		public ObjectTypeId ObjectType;
		public int RawId;

		public ExtendedUpdateFieldId(int rawId, ObjectTypeId objectType)
		{
			RawId = rawId;
			ObjectType = objectType;
		}

		public ExtendedUpdateFieldId(ObjectFields val)
		{
			RawId = (int)val;
			ObjectType = ObjectTypeId.Object;
		}

		public ExtendedUpdateFieldId(UnitFields val)
		{
			RawId = (int)val;
			ObjectType = ObjectTypeId.Unit;
		}

		public ExtendedUpdateFieldId(PlayerFields val)
		{
			RawId = (int)val;
			ObjectType = ObjectTypeId.Player;
		}

		public ExtendedUpdateFieldId(CorpseFields val)
		{
			RawId = (int)val;
			ObjectType = ObjectTypeId.Corpse;
		}

		public ExtendedUpdateFieldId(ItemFields val)
		{
			RawId = (int)val;
			ObjectType = ObjectTypeId.Item;
		}

		public ExtendedUpdateFieldId(ContainerFields val)
		{
			RawId = (int)val;
			ObjectType = ObjectTypeId.Container;
		}

		public ExtendedUpdateFieldId(DynamicObjectFields val)
		{
			RawId = (int)val;
			ObjectType = ObjectTypeId.DynamicObject;
		}

		public ExtendedUpdateFieldId(GameObjectFields val)
		{
			RawId = (int)val;
			ObjectType = ObjectTypeId.GameObject;
		}

		public static implicit operator int(ExtendedUpdateFieldId field)
		{
			return field.RawId;
		}

		public static implicit operator ExtendedUpdateFieldId(ObjectFields val)
		{
			return new ExtendedUpdateFieldId((int)val, ObjectTypeId.Object);
		}

		public static implicit operator ExtendedUpdateFieldId(UnitFields val)
		{
			return new ExtendedUpdateFieldId((int)val, ObjectTypeId.Unit);
		}

		public static implicit operator ExtendedUpdateFieldId(PlayerFields val)
		{
			return new ExtendedUpdateFieldId((int)val, ObjectTypeId.Player);
		}

		public static implicit operator ExtendedUpdateFieldId(CorpseFields val)
		{
			return new ExtendedUpdateFieldId((int)val, ObjectTypeId.Corpse);
		}

		public static implicit operator ExtendedUpdateFieldId(ItemFields val)
		{
			return new ExtendedUpdateFieldId((int)val, ObjectTypeId.Item);
		}

		public static implicit operator ExtendedUpdateFieldId(ContainerFields val)
		{
			return new ExtendedUpdateFieldId((int)val, ObjectTypeId.Container);
		}

		public static implicit operator ExtendedUpdateFieldId(DynamicObjectFields val)
		{
			return new ExtendedUpdateFieldId((int)val, ObjectTypeId.DynamicObject);
		}

		public static implicit operator ExtendedUpdateFieldId(GameObjectFields val)
		{
			return new ExtendedUpdateFieldId((int)val, ObjectTypeId.GameObject);
		}
	}
}