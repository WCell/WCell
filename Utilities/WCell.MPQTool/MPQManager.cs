using System;
using System.Collections.Generic;
using System.IO;
using MpqReader;

namespace WCell.MPQTool
{
    public class MpqManager
    {
        private readonly List<MpqArchive> MPQArchives;

        public MpqManager(string mpqPath)
        {
            MPQArchives = new List<MpqArchive>();

            var mpqNames = new List<string>
            {
                "Data\\patch-2.MPQ"
                ,"Data\\patch.MPQ"
                ,"Data\\lichking.MPQ"
                ,"Data\\expansion.MPQ"
                ,"Data\\common-2.MPQ"
                ,"Data\\common.MPQ"
                ,"Data\\enUS\\patch-enUS-2.MPQ"
                ,"Data\\enUS\\patch-enUS.MPQ"
                ,"Data\\enUS\\lichking-locale-enUS.MPQ"
                ,"Data\\enUS\\expansion-locale-enUS.MPQ"
                ,"Data\\enUS\\locale-enUS.MPQ"
                ,"Data\\enUS\\base-enUS.MPQ"
            };

            foreach (var mpqName in mpqNames)
            {
                try
                {
                    MPQArchives.Add(new MpqArchive(Path.Combine(mpqPath, mpqName)));
                }
                catch (Exception) { }
            }
        }

        public MpqStream OpenFile(string fileName)
        {
            foreach (var archive in MPQArchives)
            {
                if (archive.FileExists(fileName))
                {
                    return archive.OpenFile(fileName);
                }
            }
            throw new Exception(String.Format("Unable to load file {0}", fileName));
        }

        public bool FileExists(string fileName)
        {
            foreach (var archive in MPQArchives)
            {
                if (archive.FileExists(fileName))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
