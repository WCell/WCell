using System;
using WCell.Constants.Updates;
using WCell.Util;

namespace WCell.PacketAnalysis.Updates
{
	public class FieldRenderer
	{
		public readonly FieldRenderInfo[] Fields;
		public readonly ObjectTypeId EnumType;

		public FieldRenderer(ObjectTypeId enumType)
		{
			EnumType = enumType;

			var fields = FieldRenderUtil.GetValues(enumType);

			Fields = new FieldRenderInfo[fields.Length];

			uint i = 0;

			var fieldDefs = UpdateFieldMgr.Get(enumType);
			foreach (var e in fields)
			{
				var fieldDef = fieldDefs.Fields.Get((uint)((int)e));

			    UpdateFieldType type = fieldDef == null ? UpdateFieldType.UInt32 : fieldDef.Type;
				Fields[i] = new FieldRenderInfo(e, type);
				i++;
			}
		}

		public FieldRenderInfo GetFieldInfo(uint index)
		{
			if (index >= Fields.Length)
			{
				Console.WriteLine("Accessed invalid Field at Index {0} of {1}", index, EnumType);
				return null;
			}

			return Fields[index];
		}

		public uint Render(uint index, byte[] values, IndentTextWriter writer)
		{
			var fieldNum = index / 4;
			var field = Fields.Get(fieldNum);
			if (field != null)
			{
				string strVal;
				var len = field.Renderer(field, values, out strVal);
				writer.WriteLine(field.Name + ": " + strVal);
				return len;
			}

		    if (values[index] != 0)
		    {
		        writer.WriteLine("{0}: {1}", fieldNum, values[index]);
		    }
		    return 1;
		}

		/// <summary>
		/// Returns a string representation of the given field in the given block
		/// </summary>
		public static string Render(ExtendedUpdateFieldId fieldId, UpdateBlock block)
		{
			var renderer = FieldRenderUtil.GetRenderer(fieldId.ObjectType);

			uint fieldNum = (uint)fieldId.RawId;
			var field = renderer.Fields.Get(fieldNum);
			if (field != null)
			{
				string strVal;
				field.Renderer(field, block.Values, out strVal);
				return strVal;
			}

		    return block.Values.GetUInt32(fieldNum).ToString();
		}
	}
}