using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.Items
{
    public class ScalingStatDistributionEntry
    {
        public uint Id; // 0
        public int[] StatMod = new int[10]; // 1-10
        public uint[] Modifier = new uint[10]; // 11-20
        public uint MaxLevel; // 21
    };

    public class ScalingStatValues
    {
        public uint Id; // 0
        public uint Level; // 1
        public uint[] SsdMultiplier = new uint[6]; // 2-5 Multiplier for ScalingStatDistribution
        public uint[] ArmorMod = new uint[8]; // 6-9 Armor for level
        public uint[] DpsMod = new uint[6]; // 10-15 DPS mod for level
        public uint SpellBonus; // 16 spell power for level
        //public uint SsdMultiplier4; // 17 there's data from 3.1 dbc ssdMultiplier[3]
        //public uint SsdMultiplier5; // 18 3.3
        //public uint Unk2;                                          // 19 unk, probably also Armor for level (flag 0x80000?)
        //public uint[] ArmorMod2; // 20-23 Armor for level

        public uint GetSsdMultiplier(uint mask)
        {
            if ((mask & 0x4001F) != 0)
            {
                if ((mask & 0x00000001) != 0) return SsdMultiplier[0];
                if ((mask & 0x00000002) != 0) return SsdMultiplier[1];
                if ((mask & 0x00000004) != 0) return SsdMultiplier[2];
                if ((mask & 0x00000008) != 0) return SsdMultiplier[4];
                if ((mask & 0x00000010) != 0) return SsdMultiplier[3];
                if ((mask & 0x00040000) != 0) return SsdMultiplier[5];
            }
            return 0;
        }

        public uint GetArmorMod(uint mask)
        {
            if ((mask & 0x00F001E0) != 0)
            {
                if ((mask & 0x00000020) != 0) return ArmorMod[0];
                if ((mask & 0x00000040) != 0) return ArmorMod[1];
                if ((mask & 0x00000080) != 0) return ArmorMod[2];
                if ((mask & 0x00000100) != 0) return ArmorMod[3];

                if ((mask & 0x00100000) != 0) return ArmorMod[4]; // cloth
                if ((mask & 0x00200000) != 0) return ArmorMod[5]; // leather
                if ((mask & 0x00400000) != 0) return ArmorMod[6]; // mail
                if ((mask & 0x00800000) != 0) return ArmorMod[7]; // plate
            }
            return 0;
        }

        public uint GetDpsMod(uint mask)
        {
            if ((mask & 0x7E00) != 0)
            {
                if ((mask & 0x00000200) != 0) return DpsMod[0];
                if ((mask & 0x00000400) != 0) return DpsMod[1];
                if ((mask & 0x00000800) != 0) return DpsMod[2];
                if ((mask & 0x00001000) != 0) return DpsMod[3];
                if ((mask & 0x00002000) != 0) return DpsMod[4];
                if ((mask & 0x00004000) != 0) return DpsMod[5]; // not used?
            }
            return 0;
        }

        public uint GetSpellBonus(uint mask)
        {
            if ((mask & 0x00008000) != 0)
                return SpellBonus;
            return 0;
        }
    };
}
