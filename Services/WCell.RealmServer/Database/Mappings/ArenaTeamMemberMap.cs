using FluentNHibernate;
using FluentNHibernate.Mapping;
using WCell.RealmServer.Battlegrounds.Arenas;

namespace WCell.RealmServer.Database.Mappings
{
    class ArenaTeamMemberMap : ClassMap<ArenaTeamMember>
    {
        public ArenaTeamMemberMap()
        {
            Id(x => x.CharacterLowId);
            Map(x => x.ArenaTeamId);
            // Map the private field _name as there is no setter (rightly) for the Name property
            Map(Reveal.Member<ArenaTeamMember>("_name")).Not.Nullable();
            Map(x => x.Name).Not.Nullable();
            Map(Reveal.Member<ArenaTeamMember>("_class")).Not.Nullable();
            Map(x => x.GamesWeek).Not.Nullable();
            Map(x => x.WinsWeek).Not.Nullable();
            Map(x => x.GamesSeason).Not.Nullable();
            Map(x => x.WinsSeason).Not.Nullable();
            Map(x => x.PersonalRating).Not.Nullable();
        }
    }
}
