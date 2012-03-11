using Castle.ActiveRecord;

namespace WCell.RealmServer.Guilds
{
    public class GuildTabard
    {
        [Field]
        public int EmblemStyle;

        [Field]
        public int EmblemColor;

        [Field]
        public int BorderStyle;

        [Field]
        public int BorderColor;

        [Field]
        public int BackgroundColor;
    }
}