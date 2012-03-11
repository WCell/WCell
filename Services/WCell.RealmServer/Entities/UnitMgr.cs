using WCell.RealmServer.Content;
using WCell.RealmServer.NPCs;
using WCell.Util;
using WCell.Util.Variables;

namespace WCell.RealmServer.Entities
{
    public static class UnitMgr
    {
        [NotVariable]
        public static UnitModelInfo[] ModelInfos = new UnitModelInfo[24000];

        private static bool loaded;

        public static void InitModels()
        {
            if (loaded)
            {
                return;
            }
            loaded = true;
            ContentMgr.Load<UnitModelInfo>();
        }

        public static UnitModelInfo GetModelInfo(uint displayId)
        {
            InitModels();
            return ModelInfos.Get(displayId);
        }
    }
}