using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using MPQNav.MPQ;

namespace MPQNav.Util
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

        public static Vector2 ReadVector2(this BinaryReader br)
        {
            return new Vector2(br.ReadSingle(), br.ReadSingle());
        }

        public static Vector3 ReadVector3(this BinaryReader br)
        {
            return new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        }

        public static Vector3 ReadWMOVector3(this BinaryReader br)
        {
            var X = br.ReadSingle();
            var Y = br.ReadSingle();
            var Z = br.ReadSingle();
            return new Vector3(X, Y, Z);
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

        /// <summary>
        /// Reads a C-style null-terminated string from the current stream.
        /// </summary>
        /// <param name="binReader">the extended <see cref="BinaryReader" /> instance</param>
        /// <returns>the string being reader</returns>
        public static string ReadCString(this BinaryReader binReader)
        {
            StringBuilder sb = new StringBuilder();
            byte c;

            while ((c = binReader.ReadByte()) != 0)
            {
                sb.Append((char)c);
            }

            return sb.ToString();
        }

        public static byte PeekByte(this BinaryReader binReader)
        {
            byte b = binReader.ReadByte();
            binReader.BaseStream.Position -= 1;
            return b;
        }

        public static string ReadFixedString(this BinaryReader br, int size)
        {
            var bytes = br.ReadBytes(size);

            for (int i = 0; i < size;i++ )
            {
                if (bytes[i] == 0)
                {
                    return Encoding.ASCII.GetString(bytes, 0, i);
                }
            }

            return Encoding.ASCII.GetString(bytes);
        }

        public static bool HasData(this BinaryReader br)
        {
            return br.BaseStream.Position < br.BaseStream.Length;
        }
    }
}
