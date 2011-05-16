using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Core.ClientDB;

namespace WCell.RealmServer.Entities
{
    #region CharStartOutfit.dbc

    public sealed class CharStartOutfit
    {
        public uint Id;
        public ClassId Class;
        public RaceId Race;
        public GenderType Gender;
        // byte unk always 0
        public uint[] ItemIds;//12
        //public uint[] StartingItemDisplayIds;//12
        public InventorySlotType[] ItemSlots;//12
    }

    public sealed class CharStartOutfitConverter : AdvancedClientDBRecordConverter<CharStartOutfit>
    {
        public override CharStartOutfit ConvertTo(byte[] rawData, ref int id)
        {
            id = GetInt32(rawData, 0);

            int currIndex = 0;

            var outfit = new CharStartOutfit();
            outfit.Id = GetUInt32(rawData, currIndex++);
            uint temp = GetUInt32(rawData, currIndex++);

            outfit.Race = (RaceId)(temp & 0xFF);
            outfit.Class = (ClassId)((temp & 0xFF00) >> 8);
            outfit.Gender = (GenderType)((temp & 0xFF0000) >> 16);

            for (int i = 0; i < 12; i++)
            {
                outfit.ItemIds[i] = GetUInt32(rawData, currIndex++);
            }

            // Skip display ids
            currIndex += 12;

            for (int i = 0; i < 12; i++)
            {
                outfit.ItemSlots[i] = (InventorySlotType)GetUInt32(rawData, currIndex++);
            }

            return outfit;
        }
    }

    #endregion
}
