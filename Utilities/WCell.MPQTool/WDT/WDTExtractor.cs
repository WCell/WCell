using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SCSharp;

namespace WCell.MPQTool.WDT
{
	/// <summary>
	/// World\Maps\*MapInternalName*\*MapInternalName*.wdt, .wdl, _XX_YY.adt
	/// common-2.mpq, expansion.mpq, and lichking.mpq for the base, then overwrite from patch.mpq, then patch-2.mpq
	/// </summary>
	public class WDTExtractor
	{
		// 
		//private readonly MpqArchive archive;
		//private readonly BinaryReader file;
		//private readonly string name;
		//private readonly Stream stream;
		//public bool loaded;

		//public WDTExtractor(MpqArchive archive)
		//{
		//    var wdtfile = "World\\Maps\\" + name + "\\" + name + ".wdt";
		//    if (!archive.ExtractFile(wdtfile, "PPather\\wdt.tmp"))
		//        return;

		//    loaded = true;
		//    this.name = name;
		//    this.archive = archive;

		//    stream = File.OpenRead("PPather\\wdt.tmp");
		//    file = new BinaryReader(stream);

		//    bool done = false;
		//    do
		//    {
		//        try
		//        {
		//            uint type = file.ReadUInt32();
		//            uint size = file.ReadUInt32();
		//            long curpos = file.BaseStream.Position;

		//            if (type == ChunkReader.MVER)
		//            {
		//            }
		//            else if (type == ChunkReader.MPHD)
		//            {
		//            }
		//            else if (type == ChunkReader.MODF)
		//            {
		//                HandleMODF(size);
		//            }
		//            else if (type == ChunkReader.MWMO)
		//            {
		//                HandleMWMO(size);
		//            }
		//            else if (type == ChunkReader.MAIN)
		//            {
		//                HandleMAIN(size);
		//            }
		//            else
		//            {
		//                Console.WriteLine("Found unknown WDT Chunk: " + type);
		//                //done = true; 
		//            }
		//            file.BaseStream.Seek(curpos + size, SeekOrigin.Begin);
		//        }
		//        catch (EndOfStreamException)
		//        {
		//            done = true;
		//        }
		//    } while (!done);

		//    file.Close();
		//    stream.Close();

		//    // load map tiles
		//}

		//public void LoadMapTile(int x, int y)
		//{
		//    if (wdt.maps[x, y])
		//    {
		//        var t = new MapTile();

		//        var filename = "World\\Maps\\" + name + "\\" + name + "_" + x + "_" + y + ".adt";
		//        if (archive.ExtractFile(filename, "PPather\\adt.tmp"))
		//        {
		//            PPather.WriteLine("Reading adt: " + filename);
		//            var f = new MapTileFile("PPather\\adt.tmp", t, wmomanager, modelmanager);
		//            if (t.models.Count != 0 || t.wmos.Count != 0)
		//            {
		//                //Console.WriteLine(name + " " + x + " " + z + " models: " + t.models.Count + " wmos: " + t.wmos.Count);
		//                // Weee
		//            }
		//            wdt.maptiles[x, y] = t;
		//        }
		//    }
		//}

		//private void HandleMWMO(uint size)
		//{
		//    if (size != 0)
		//    {
		//        int l = 0;
		//        byte[] raw = file.ReadBytes((int)size);
		//        while (l < size)
		//        {
		//            string s = ChunkReader.ExtractString(raw, l);
		//            l += s.Length + 1;
		//            wdt.gwmos.Add(s);
		//        }
		//    }
		//}

		//private void HandleMODF(uint size)
		//{
		//    // global wmo instance data
		//    wdt.gnWMO = (int)size / 64;
		//    for (uint i = 0; i < wdt.gnWMO; i++)
		//    {
		//        int id = file.ReadInt32();
		//        string path = wdt.gwmos[id];

		//        WMO wmo = wmomanager.AddAndLoadIfNeeded(path);

		//        var wmoi = new WMOInstance(wmo, file);
		//        wdt.gwmois.Add(wmoi);
		//    }
		//}

		//private void HandleMAIN(uint size)
		//{
		//    // global map objects
		//    for (int j = 0; j < 64; j++)
		//    {
		//        for (int i = 0; i < 64; i++)
		//        {
		//            int d = file.ReadInt32();
		//            if (d != 0)
		//            {
		//                wdt.maps[i, j] = true;
		//                wdt.nMaps++;
		//            }
		//            else
		//                wdt.maps[i, j] = false;
		//            file.ReadInt32(); // kasta
		//        }
		//    }
		//}
	}
}
