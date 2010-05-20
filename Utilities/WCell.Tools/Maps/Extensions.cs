using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WCell.Util.Graphics;

namespace WCell.Tools.Maps
{
    public static class Extensions
    {
        //public static void WritePacked(this BinaryWriter writer, OBB obb)
        //{
        //    writer.WritePacked(obb.Bounds);
        //    writer.WritePacked(obb.InverseRotation);
        //}

        //public static void WritePacked(this BinaryWriter writer, BoundingBox box)
        //{
        //    writer.WritePacked(box.Min);
        //    writer.WritePacked(box.Max);
        //}

        //public static void WritePacked(this BinaryWriter writer, Vector3 vec)
        //{
        //    writer.WritePacked(vec.X);
        //    writer.WritePacked(vec.Y);
        //    writer.WritePacked(vec.Z);
        //}

        //public static void WritePacked(this BinaryWriter writer, Matrix mat)
        //{
        //    writer.WritePacked(mat.M11);
        //    writer.WritePacked(mat.M12);
        //    writer.WritePacked(mat.M13);
        //    writer.WritePacked(mat.M14);
        //    writer.WritePacked(mat.M21);
        //    writer.WritePacked(mat.M22);
        //    writer.WritePacked(mat.M23);
        //    writer.WritePacked(mat.M24);
        //    writer.WritePacked(mat.M31);
        //    writer.WritePacked(mat.M32);
        //    writer.WritePacked(mat.M33);
        //    writer.WritePacked(mat.M34);
        //    writer.WritePacked(mat.M41);
        //    writer.WritePacked(mat.M42);
        //    writer.WritePacked(mat.M43);
        //    writer.WritePacked(mat.M44);
        //}

        public static void Write(this BinaryWriter writer, Index3 idx)
        {
            writer.Write(idx.Index0);
            writer.Write(idx.Index1);
            writer.Write(idx.Index2);
        }

        public static void Write(this BinaryWriter writer, Vector3 vector3)
        {
            writer.Write(vector3.X);
            writer.Write(vector3.Y);
            writer.Write(vector3.Z);
        }

        public static void Write(this BinaryWriter writer, BoundingBox box)
        {
            writer.Write(box.Min);
            writer.Write(box.Max);
        }

        public static Index3 ReadIndex3(this BinaryReader reader)
        {
            var idx0 = reader.ReadInt16();
            var idx1 = reader.ReadInt16();
            var idx2 = reader.ReadInt16();

            return new Index3
            {
                Index0 = idx0,
                Index1 = idx1,
                Index2 = idx2
            };
        }

        //public static BoundingBox ReadPackedBoundingBox(this BinaryReader reader)
        //{
        //    var min = reader.ReadPackedVector3();
        //    var max = reader.ReadPackedVector3();
        //    return new BoundingBox(min, max);
        //}

        //public static Vector3 ReadPackedVector3(this BinaryReader reader)
        //{
        //    var X = reader.ReadPackedFloat();
        //    var Y = reader.ReadPackedFloat();
        //    var Z = reader.ReadPackedFloat();
        //    return new Vector3(X, Y, Z);
        //}

        //public static OBB ReadPackedOBB(this BinaryReader reader)
        //{
        //    var box = reader.ReadPackedBoundingBox();
        //    var mat = reader.ReadPackedMatrix();

        //    return new OBB
        //    {
        //        Bounds = box,
        //        InverseRotation = mat
        //    };
        //}

        //public static Matrix ReadPackedMatrix(this BinaryReader reader)
        //{
        //    var M11 = reader.ReadPackedFloat();
        //    var M12 = reader.ReadPackedFloat();
        //    var M13 = reader.ReadPackedFloat();
        //    var M14 = reader.ReadPackedFloat();
        //    var M21 = reader.ReadPackedFloat();
        //    var M22 = reader.ReadPackedFloat();
        //    var M23 = reader.ReadPackedFloat();
        //    var M24 = reader.ReadPackedFloat();
        //    var M31 = reader.ReadPackedFloat();
        //    var M32 = reader.ReadPackedFloat();
        //    var M33 = reader.ReadPackedFloat();
        //    var M34 = reader.ReadPackedFloat();
        //    var M41 = reader.ReadPackedFloat();
        //    var M42 = reader.ReadPackedFloat();
        //    var M43 = reader.ReadPackedFloat();
        //    var M44 = reader.ReadPackedFloat();

        //    return new Matrix(M11, M12, M13, M14, 
        //                      M21, M22, M23, M24,
        //                      M31, M32, M33, M34,
        //                      M41, M42, M43, M44);
        //}

        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            var X = reader.ReadSingle();
            var Y = reader.ReadSingle();
            return new Vector2(X, Y);
        }

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

        //public static void WritePacked(this BinaryWriter writer, float f)
        //{
        //    var packed = HalfUtils.Pack(f);
        //    writer.Write(packed);
        //}

        //public static float ReadPackedFloat(this BinaryReader reader)
        //{
        //    var packed = reader.ReadUInt16();
        //    return HalfUtils.Unpack(packed);
        //}

        public static bool HasFlag(this WDTFlags flags, WDTFlags flag)
        {
            return ((flags & flag) != 0);
        }
    }
}
