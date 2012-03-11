using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WCell.MPQTool.StormLibWrapper;

//using MpqReader;

namespace WCell.MPQTool
{
    /// <summary>
    /// Provides access to all archives in a path.
    /// </summary>
    public class MpqLibrarian
    {
        private static MpqLibrarian DefaultLibrarian;

        public static MpqLibrarian GetDefaultFinder(string wowPath)
        {
            if (DefaultLibrarian == null)
            {
                DefaultLibrarian = new MpqLibrarian(DBCTool.FindWowDirOrThrow(wowPath));
            }
            return DefaultLibrarian;
        }

        public readonly List<MpqArchive> MPQArchives;

        public MpqLibrarian(string mpqPath)
        {
            MPQArchives = new List<MpqArchive>();

            var mpqNames = new List<string>
            {
                "Data\\patch-3.MPQ"
                ,"Data\\patch-2.MPQ"
                ,"Data\\patch.MPQ"
                ,"Data\\lichking.MPQ"
                ,"Data\\expansion.MPQ"
                ,"Data\\common-2.MPQ"
                ,"Data\\common.MPQ"
            };

            var dataDirectory = Path.Combine(mpqPath, "Data\\");
            var localeDirectorys = Directory.EnumerateDirectories(dataDirectory);

            foreach (var localeDir in localeDirectorys)
            {
                var locale = localeDir.Substring(localeDir.Length - 4, 4);
                switch (locale)
                {
                    case "enUS":
                    case "enGB":
                    case "koKR":
                    case "frFR":
                    case "deDE":
                    case "zhCN":
                    case "zhTW":
                    case "esES":
                    case "esMX":
                    case "ruRU":
                        break;
                    default:
                        continue;
                }
                mpqNames.Add(Path.Combine("Data\\", locale, "patch-" + locale + "-3.MPQ"));
                mpqNames.Add(Path.Combine("Data\\", locale, "patch-" + locale + "-2.MPQ"));
                mpqNames.Add(Path.Combine("Data\\", locale, "patch-" + locale + ".MPQ"));
                mpqNames.Add(Path.Combine("Data\\", locale, "lichking-locale-" + locale + ".MPQ"));
                mpqNames.Add(Path.Combine("Data\\", locale, "expansion-locale-" + locale + ".MPQ"));
                mpqNames.Add(Path.Combine("Data\\", locale, "locale-" + locale + ".MPQ"));
                mpqNames.Add(Path.Combine("Data\\", locale, "base-" + locale + ".MPQ"));
            }

            foreach (var mpqName in mpqNames)
            {
                try
                {
                    var path = Path.Combine(mpqPath, mpqName);
                    if (File.Exists(path))
                        MPQArchives.Add(new MpqArchive(path));
                    else
                        Console.WriteLine("File not found: {0}", path);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception thrown in MpqManager constructor." + e);
                }
            }
        }

        public MpqStream OpenFile(string fileName)
        {
            foreach (var archive in MPQArchives)
            {
                if (archive.FileExists(fileName))
                {
                    return archive.OpenFile(fileName).GetStream();
                }
            }
            throw new Exception(String.Format("Could not find file \"{0}\" in any of the {1} archives",
                    fileName,
                    MPQArchives.Count));
        }

        public bool FileExists(string fileName)
        {
            return MPQArchives.Any(archive => archive.FileExists(fileName));
        }

        public IEnumerable<string> GetAllFiles(string match)
        {
            return MPQArchives.SelectMany(archive => archive.FindAllFiles(match));
        }
    }
}