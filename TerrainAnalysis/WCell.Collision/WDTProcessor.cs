using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections;
using Microsoft.DirectX;

namespace WCell.Collision
{
    public class WDTImporter : ChunkedFileProcessor 
    {
        private WDT wdt;

        public WDTImporter(string fileName) : base(fileName)
        {
            wdt = new WDT();
            wdt.fileName = fileName;
        }

        public WDT WDTData
        {
            get
            {
                return this.wdt;
            }
        }

        protected override void ProcessChunk(string chunkSignature, ref int chunkSize)
        {
            switch (chunkSignature)
            {
                case "MVER":
                    ReadMVER(m_binReader, this.wdt);
                    break;
                case "MPHD":                    
                    ReadMPHD(m_binReader, this.wdt);
                    break;                    
                case "MAIN":                    
                    ReadMAIN(m_binReader, this.wdt);
                    break;                    
                case "MWMO":                   
                    ReadMWMO(m_binReader, this.wdt, chunkSize);
                    break;                    
                case "MODF":                    
                    break;
                    
            }
        }

        protected override void PostProcess()
        {
            base.PostProcess();
        }

        private void ReadMVER(BinaryReader br, WDT m_wdt)
        {
            m_wdt.Version = br.ReadInt32();
        }

        private void ReadMPHD(BinaryReader br, WDT m_wdt)
        {
            int terrainInt = br.ReadInt32();
            if (terrainInt == 1)
                m_wdt.HasTerrain = false;
            else
                m_wdt.HasTerrain = true;
        }

        private void ReadMAIN(BinaryReader br, WDT m_wdt)
        {
            m_wdt.TileProfile = new byte[64, 64];
            for (int i = 0; i < 64; i++)
            {
                for (int j = 0; j < 64; j++)
                {
                    Int64 tileFlag = br.ReadInt64();
                    m_wdt.TileProfile[i, j] = (byte)tileFlag;
                }
            }
        }

        private void ReadMWMO(BinaryReader br, WDT m_wdt, int listSize)
        {
            ArrayList strings = new ArrayList();
            long start = br.BaseStream.Position;
            byte[] tempString = new byte[256];
            int charIndex=0;
            while (br.BaseStream.Position < start + listSize)
            {
                byte tempChar;
                charIndex = 0;
                while ((tempChar = br.ReadByte()) != 0)
                {
                    tempString[charIndex++] = tempChar;
                }
                strings.Add(Encoding.ASCII.GetString(tempString,0,charIndex));
            }

            m_wdt.WMOFiles = new string[strings.Count];
            int index=0;
            foreach (string fName in strings)
                m_wdt.WMOFiles[index++] = fName;
        }

        private void ReadMODF(BinaryReader br, WDT m_wdt, int listSize)
        {
            ArrayList tempList = new ArrayList();
            long start = br.BaseStream.Position;
            while (br.BaseStream.Position < start + listSize)
                tempList.Add(br.ReadStruct<WMOInstance>());

            m_wdt.WMOList = new WMOInstance[tempList.Count];
            int index = 0;
            foreach (WMOInstance wmo in tempList)
                m_wdt.WMOList[index++] = wmo;
        }
    }

    public class WDT
    {
        public string fileName;

        #region MVER
        public int Version;
        #endregion
        #region MPHD
        public bool HasTerrain;
        #endregion

        #region MAIN
        public byte[,] TileProfile;
        #endregion

        #region MWMO
        public string[] WMOFiles;
        #endregion

        #region MODF
        public WMOInstance[] WMOList;
        #endregion        
    }


    [StructLayout(LayoutKind.Sequential, Size = 0x40)]
    public struct WMOInstance
    {
        int MWMOIndex;
        int uniqueIdentifer;
        Vector3 Position;
        Vector3 Orientation;

        float Ext1;
        float Ext2;
        float Ext3;
        float Ext4;
        float Ext5;
        float Ext6;

        int Flags;
        Int16 doodadSetIndex;
        int nameSet;
    }
}
