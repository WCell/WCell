using WCell.Constants.Misc;
using WCell.Core;
using WCell.Core.DBC;
using WCell.Core.Initialization;
using WCell.Util.Data;

namespace WCell.RealmServer.Chat
{
    public static class EmoteDBC
    {
        [NotPersistent]
        public static MappedDBCReader<EmoteType, EmoteRelationConverter> EmoteRelationReader;

        [Initialization(InitializationPass.First, null)]
        public static void LoadEmotes()
        {
            EmoteRelationReader = new MappedDBCReader<EmoteType, EmoteRelationConverter>(RealmServerConfiguration.GetDBCFile(WCellDef.DBC_EMOTESTEXT));
        }

        /// <summary>
        /// Emote relation holder, searches via TextEmote
        /// </summary>
        public class EmoteRelationConverter : AdvancedDBCRecordConverter<EmoteType>
        {
            public override EmoteType ConvertTo(byte[] rawData, ref int id)
            {
                id = GetInt32(rawData, 0); //-TextEmote

                return (EmoteType)GetUInt32(rawData, 2);
            }
        }
    }
}