using System.Collections.Generic;
using WCell.Core;
using WCell.Core.DBC;

namespace WCell.RealmServer.Misc
{
    public static class CfgCategories
    {
        public static Dictionary<int, string> ReadCategories()
        {
            var reader = new MappedDBCReader<string, DBCCtfCategoriesConverter>(RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_CFGCATEGORIES));
            return reader.Entries;
        }
    }

    public class DBCCtfCategoriesConverter : AdvancedDBCRecordConverter<string>
    {
        public override string ConvertTo(byte[] rawData, ref int id)
        {
            id = (int)GetUInt32(rawData, 0);
            var name = GetString(rawData, 4);
            return name;
        }
    }
}