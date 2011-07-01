using System.Collections.Generic;
using System.IO;
using WCell.Terrain.GUI.Util;
using WCell.Terrain.MPQ;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.Terrain
{
	public static class Extensions
	{

		public static OffsetLocation ReadOffsetLocation(this BinaryReader br)
		{
			return new OffsetLocation
			{
				Count = br.ReadInt32(),
				Offset = br.ReadInt32()
			};
		}

		public static void ReadOffsetLocation(this BinaryReader br, ref OffsetLocation offsetLoc)
		{
			offsetLoc = new OffsetLocation
			{
				Count = br.ReadInt32(),
				Offset = br.ReadInt32()
			};
		}

		public static Index3 ReadIndex3(this BinaryReader br)
		{
			return new Index3
			{
				Index0 = br.ReadInt16(),
				Index1 = br.ReadInt16(),
				Index2 = br.ReadInt16()
			};
		}

		public static Quaternion ReadQuaternion(this BinaryReader br)
		{
			return new Quaternion(br.ReadVector3(), br.ReadSingle());
		}

		public static BoundingBox ReadBoundingBox(this BinaryReader br)
		{
			return new BoundingBox(br.ReadVector3(), br.ReadVector3());
		}

		public static Plane ReadPlane(this BinaryReader br)
		{
			return new Plane(br.ReadVector3(), br.ReadSingle());
		}

		public static Color4 ReadColor4(this BinaryReader br)
		{
			return new Color4
			{
				B = br.ReadByte(),
				G = br.ReadByte(),
				R = br.ReadByte(),
				A = br.ReadByte()
			};
		}

		public static List<Index3> ReadIndex3List(this BinaryReader br)
		{
			var count = br.ReadInt32();
			var list = new List<Index3>(count);
			for (var i = 0; i < count; i++)
			{
				list.Add(br.ReadIndex3());
			}
			return list;
		}

		public static void Write(this BinaryWriter writer, Index3 idx)
		{
			writer.Write(idx.Index0);
			writer.Write(idx.Index1);
			writer.Write(idx.Index2);
		}

		public static void Write(this BinaryWriter writer, ICollection<Index3> list)
		{
			writer.Write(list.Count);
			foreach (var item in list)
			{
				writer.Write(item);
			}
		}
	}
}
