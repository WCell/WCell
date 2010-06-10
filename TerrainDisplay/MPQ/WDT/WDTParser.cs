using System;
using System.IO;
using System.Windows.Forms;
using MPQNav.MPQ.ADT;
using MPQNav.Util;

namespace MPQNav.MPQ.WDT
{
    
    public class WDTParser
    {
        private const string _wdtPath = "WORLD\\MAPS\\";
        private static string _basePath;
        private static ContinentType _continent;

        public static WDTFile Process(string dataDirectory, ContinentType internalMapName )
        {
            if (Directory.Exists(dataDirectory))
            {
                _basePath = Path.Combine(dataDirectory, _wdtPath);
                _continent = internalMapName;
            }
            else
            {
				var msg = "Invalid data directory entered. Please exit and update your app.CONFIG file";
                MessageBox.Show(msg,
                                "Invalid Data Directory");
            	throw new Exception(msg);
            }

            var wdt = new WDTFile();

            var continentPath = Path.Combine(_basePath, _continent.ToString());
            if (!Directory.Exists(continentPath))
            {
                throw new Exception(String.Format("Continent data missing for {0}", _continent));
            }

            var filePath = string.Format("{0}\\{1}.wdt", continentPath, _continent);
            if (!File.Exists(filePath))
            {
                throw new Exception("WDT doesn't exist: " + filePath);
            }

            var fileReader = new BinaryReader(File.OpenRead(filePath));

            ReadMVER(fileReader, wdt);
            ReadMPHD(fileReader, wdt);

            if (wdt.Header.IsWMOMap)
            {
                // No terrain, the map is a "global" wmo
                // MWMO and MODF chunks follow
            }
            else
            {
                ReadMAIN(fileReader, wdt);
                //PrintProfile(wdt);
            }

            do
            {
                var type = fileReader.ReadUInt32();
                var size = fileReader.ReadUInt32();
                var curpos = fileReader.BaseStream.Position;

                fileReader.BaseStream.Seek(curpos + size, SeekOrigin.Begin);

            } while (fileReader.HasData());

            fileReader.Close();

            return wdt;
        }


        static void ReadMVER(BinaryReader fileReader, WDTFile wdt)
        {
            var type = fileReader.ReadUInt32();
            var size = fileReader.ReadUInt32();
            wdt.Version = fileReader.ReadInt32();
        }

        static void ReadMPHD(BinaryReader fileReader, WDTFile wdt)
        {
            var type = fileReader.ReadUInt32();
            var size = fileReader.ReadUInt32();
            wdt.Header.Header1 = (WDTFlags)fileReader.ReadInt32();
            wdt.Header.Header2 = fileReader.ReadInt32();
            wdt.Header.Header3 = fileReader.ReadInt32();
            wdt.Header.Header4 = fileReader.ReadInt32();
            wdt.Header.Header5 = fileReader.ReadInt32();
            wdt.Header.Header6 = fileReader.ReadInt32(); 
            wdt.Header.Header7 = fileReader.ReadInt32();
            wdt.Header.Header8 = fileReader.ReadInt32();
        }

        static void ReadMAIN(BinaryReader fileReader, WDTFile wdt)
        {
            var type = fileReader.ReadUInt32();
            var size = fileReader.ReadUInt32();

            // Rows are along the x-axis
            for (var x = 0; x < 64; x++)
            {
                // Columns are along the y-axis
                for (var y = 0; y < 64; y++)
                {
                    // Stored as [col, row], that's weird.
                    wdt.TileProfile[y, x] = fileReader.ReadInt64() != 0;
                }
            }
        }

        static void PrintProfile(WDTFile wdt)
        {
            using (var file = new StreamWriter(_continent + ".wdtprofile.txt"))
            {
                for (var x = 0; x < 64; x++)
                {
                    for (var y = 0; y < 64; y++)
                    {
                        if (wdt.TileProfile[y,x])
                        {
                            file.Write("X");
                        }
                        else
                        {
                            file.Write(".");
                        }
                    }
                    file.WriteLine();
                }
            }
        }
    }
}
