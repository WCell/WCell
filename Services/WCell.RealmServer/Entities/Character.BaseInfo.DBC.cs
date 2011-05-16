using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Core.ClientDB;

namespace WCell.RealmServer.Entities
{
    public struct CharBaseInfo
    {
        public RaceId Race;
        public ClassId Class;
    }

    public sealed class CharBaseInfoConverter : AdvancedClientDBRecordConverter<CharBaseInfo>
    {
        public override CharBaseInfo ConvertTo(byte[] rawData, ref int id)
        {
            id = 0;

            var cbi = new CharBaseInfo
                          {
                              Race = (RaceId) rawData[0],
                              Class = (ClassId) rawData[1]
                          };

            return cbi;
        }
    }
}
