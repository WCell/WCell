using System;
using System.Collections.Generic;

///
/// This file was automatically created, using WCell.Tools
///

namespace WCell.Constants.Updates
{
	public class UpdateFieldCollection
	{
		public readonly UpdateField[] Fields;
		public readonly UpdateFieldFlags[] FieldFlags;
		public readonly int[] OwnerIndices;
		public readonly int[] GroupIndices;
		public readonly int[] DynamicIndices;

		public readonly ObjectTypeId TypeId;
		public readonly UpdateFieldCollection BaseCollection;
		public readonly int Offset;
		public readonly bool HasPrivateFields;

		internal UpdateFieldCollection(ObjectTypeId id, UpdateField[] fields,
			UpdateFieldCollection baseCollection, int offset, bool hasPrivateFields)
		{
			TypeId = id;
			Fields = fields;
			FieldFlags = new UpdateFieldFlags[fields.Length];
			var ownerIndices = new List<int>(25);
			var groupIndices = new List<int>(25);
			var dynamicIndices = new List<int>(25);
			for (var i = 0; i < Fields.Length; i++)
			{
				var field = Fields[i];
				FieldFlags[i] = field.Flags;
				if ((field.Flags & UpdateFieldFlags.Dynamic) != 0)
				{
					dynamicIndices.Add(i);
				}
				else
				{
					if ((field.Flags & UpdateFieldFlags.OwnerOnly) != 0)
					{
						ownerIndices.Add(i);
					}
					if ((field.Flags & UpdateFieldFlags.GroupOnly) != 0)
					{
						groupIndices.Add(i);
					}
				}
			}
			OwnerIndices = ownerIndices.ToArray();
			GroupIndices = groupIndices.ToArray();
			DynamicIndices = dynamicIndices.ToArray();
			BaseCollection = baseCollection;
			Offset = offset;
			HasPrivateFields = hasPrivateFields;
		}

		public int Length
		{
			get
			{
				return Fields.Length - Offset;
			}
		}

		public int TotalLength
		{
			get
			{
				return Fields.Length;
			}
		}
	}

}
