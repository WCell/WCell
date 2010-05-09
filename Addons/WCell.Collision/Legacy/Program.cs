using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using WCell.Util.Graphics;

namespace WCell.Collision
{
    class Program
    {
        static void Main(string[] args)
        {
            //var tile = WorldMap.GetTile(530, 26, 12);

            //ADTImporter importer = new ADTImporter("Expansion01_12_26.adt");
            //importer.Process();
            //ADT adt = importer.ADT;

            //WMORoot root = WMORootImporter.Import("BearGod_HighPriest.wmo");

            //root.DumpInfo(Console.Out);

            //WMOGroup group = WMOGroupProcessor.Import("BearGod_HighPriest_000.wmo");

            //group.DumpInfo(Console.Out);

            Console.ReadLine();

            Vector3 endPoint = Vector3.Forward;
            Vector3 point = Vector3.Down;
            uint x = (uint)((uint)((endPoint.X - point.X) * 4f) & 0x7FF);
            uint y = (uint)(((uint)((endPoint.Y - point.Y) * 4f) << 11) & 0x3FF800);
            uint z = (uint)(((uint)((endPoint.Z - point.Z) * 4f) << 22) & 0xFFC00000);
            //return (uint) (((((int) ((endPoint.X - point.X) * 4f)) & 2047) + ((((int) ((endPoint.Y - point.Y) * 4f)) << 11) & 4192256)) + ((((int) ((endPoint.Z - point.Z) * 4f)) << 22) & 4290772992L));
        }
    }
}
