using System;

namespace WCell.Constants.Updates
{
	public static class UpdateFieldMgr
	{
		public static readonly UpdateFieldCollection[] Collections = new UpdateFieldCollection[UpdateField.ObjectTypeCount];
		public static readonly ObjectTypeId[] InheritedTypeIds = new ObjectTypeId[UpdateField.ObjectTypeCount];

		public static int ExplorationZoneFieldSize;

		public static void Init()
		{
			InitInheritance();
			FixFields();

			for (var i = ObjectTypeId.Object; i < (ObjectTypeId)UpdateField.ObjectTypeCount; i++)
			{
				var fields = (UpdateField[])UpdateFields.AllFields[(int)i].Clone();

				var offset = int.MaxValue;
				var hasPrivateFields = false;
				UpdateField lastField = null;
				for (var k = 0; k < fields.Length; k++)
				{
					var field = fields[k];
					if (field != null)
					{
						if (offset == int.MaxValue)
						{
							offset = (int)field.Offset;
						}
						lastField = field;
						hasPrivateFields = hasPrivateFields || (field.Flags & UpdateFieldFlags.Private) != UpdateFieldFlags.None;
					}
					else
					{
						if (lastField != null)
						{
							fields[k] = lastField;
						}
					}
				}


				var baseType = InheritedTypeIds[(int)i];

				UpdateFieldCollection baseCollection;
				if (baseType != ObjectTypeId.None)
				{
					baseCollection = Collections[(int)baseType];

					if (baseCollection.Fields.Length >= fields.Length)
					{
						throw new Exception("BaseCollection of UpdateFields equal or bigger than inherited collection");
					}

					// copy all inherited fields into this Collection's array
					for (var j = 0; j < baseCollection.Fields.Length; j++)
					{
						var field = baseCollection.Fields[j];
						fields[j] = field;
					}
				}
				else
				{
					baseCollection = null;
				}

				Collections[(int)i] = new UpdateFieldCollection(i, fields, baseCollection, offset, hasPrivateFields);
			}
		}

		/// <summary>
		/// Looks a little ugly but sadly is very important
		/// </summary>
		private static void FixFields()
		{
			//ExplorationZoneFieldSize = Get(ObjectTypeId.Player).Fields[(uint)PlayerFields.EXPLORED_ZONES_1].Size;
			ExplorationZoneFieldSize = (int) UpdateFields.AllFields[(int)ObjectTypeId.Player][(uint)PlayerFields.EXPLORED_ZONES_1].Size;
		}

		private static void InitInheritance()
		{
			InheritedTypeIds[(int)ObjectTypeId.Object] = ObjectTypeId.None;
			InheritedTypeIds[(int)ObjectTypeId.Item] = ObjectTypeId.Object;
			InheritedTypeIds[(int)ObjectTypeId.Container] = ObjectTypeId.Item;
			InheritedTypeIds[(int)ObjectTypeId.Unit] = ObjectTypeId.Object;
			InheritedTypeIds[(int)ObjectTypeId.Player] = ObjectTypeId.Unit;
			InheritedTypeIds[(int)ObjectTypeId.GameObject] = ObjectTypeId.Object;
			InheritedTypeIds[(int)ObjectTypeId.DynamicObject] = ObjectTypeId.Object;
			InheritedTypeIds[(int)ObjectTypeId.Corpse] = ObjectTypeId.Object;
		}

		public static UpdateFieldCollection Get(ObjectTypeId type)
		{
			if (Collections[0] == null)
			{
				Init();
			}
			return Collections[(int)type];
		}
	}
}