using System;
using System.IO;
using WCell.Constants.Updates;

namespace WCell.PacketAnalysis.Updates
{

	public class FieldRenderInfo
	{
		public readonly object Field;
		public readonly UpdateFieldType Type;
		public readonly string Name;
		public RenderHandler Renderer;

		public readonly uint Index;

		public FieldRenderInfo(object field, UpdateFieldType type)
		{
			Field = field;
			Type = type;
			Name = FieldRenderUtil.GetFriendlyName(field);

			FieldRenderUtil.CustomRenderers.TryGetValue(field, out Renderer);
			if (Renderer == null)
			{
				Renderer = FieldRenderUtil.TypeRenderers[(int)Type];
			}

			Index = Convert.ToUInt32(field);
		}

		public uint Length
		{
			get
			{
				return FieldRenderUtil.GetSizeof(Type);
			}
		}

		public void Render(object value, string indent, TextWriter writer)
		{
			writer.WriteLine(indent + "{0}: {1}", Name, value);
		}

		public override string ToString()
		{
			return "Renderer for " + Name;
		}
	}
}