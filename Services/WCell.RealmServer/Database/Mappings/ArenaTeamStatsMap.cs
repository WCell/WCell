using FluentNHibernate.Mapping;
using WCell.RealmServer.Battlegrounds.Arenas;

namespace WCell.RealmServer.Database.Mappings
{
    public class ArenaTeamStatsMap : ClassMap<ArenaTeamStats>
    {
        public ArenaTeamStatsMap()
        {
            Id(x => x.Id);
            Map(x => x.Rating).Not.Nullable();
            Map(x => x.GamesWeek).Not.Nullable();
            Map(x => x.WinsWeek).Not.Nullable();
            Map(x => x.GamesSeason).Not.Nullable();
            Map(x => x.WinsSeason).Not.Nullable();
            Map(x => x.Rank).Not.Nullable();
        }
    }
}
