//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Microsoft.Xna.Framework;
//using System.Runtime.InteropServices;

//namespace WCell.Collision
//{
//    class WLQProcessor
//    {

//    }

//    public class WLQClass
//    {
//        public uint Signature; // LIQ2
//        public uint Field_4;
//        public uint Field_8;
//        public uint Field_C;
//        public uint Field_10;
//        public uint Field_14;
//        public uint Field_18;
//        public uint Field_1C;
//        public ushort Field_20;
//        public uint ChunkCount;// 0x22
//        public WLQChunk[] DataChunks;

//        public class WLQChunk
//        {
//            public Vector3[] DataArray = new Vector3[64];
//            public float Float1;
//            public float Float2;
//            public uint[] UnknownArray = new uint[40];
//        }
//    }

//    public class WLW
//    {
//        public uint Signature; // LIQ*
//        public uint Field_4;
//        public uint Field_8;
//        public uint ChunkCount;
//        public WLWChunk[] DataChunks;

//        public class WLWChunk
//        {
//            public Vector3[] DataArray = new Vector3[64];
//            public float Float1;
//            public float Float2;
//            public uint[] UnknownArray = new uint[40];
//        }
//    }
//}
