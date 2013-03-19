using FluentNHibernate;
using FluentNHibernate.Mapping;
using WCell.Constants;
using WCell.RealmServer.Database.Entities;

namespace WCell.RealmServer.Database.Mappings
{
    internal class CharacterRecordMap : ClassMap<CharacterRecord>
    {
        /// <summary>
        /// Character will not have Ids below this threshold. 
        /// You can use those unused ids for self-implemented mechanisms, eg to fake participants in chat-channels etc.
        /// </summary>
        /// <remarks>
        /// Do not change this value once the first Character exists.
        /// If you want to change this value to reserve more (or less) ids for other use, make sure
        /// that none of the ids below this threshold are in the DB.
        /// </remarks>
        public const long LowestCharId = 1000;

        public CharacterRecordMap()
        {
            Id(x => x.EntityLowId).GeneratedBy.Increment().Default(LowestCharId);
            Map(x => x.AccountId).Not.Nullable();
            Map(x => x.DisplayId).Not.Nullable();
            Map(x => x.WatchedFaction).Not.Nullable();
            Map(x => x.Class).Not.Nullable();
            Map(x => x.MapId).Not.Nullable();
            Map(x => x.CorpseMap).Not.Nullable();
            Map(x => x.Zone);
            Map(x => x.BindZone);
            Map(x => x.BindMap).Not.Nullable();
            Map(x => x.Name).Unique().Length(12).Not.Nullable();
            Map(x => x.Created).Not.Nullable();
            Map(x => x.LastLogin).Nullable();
            Map(x => x.LastLogout).Nullable();
            Map(x => x.CharacterFlags).Not.Nullable();
            Map(x => x.Race).Not.Nullable();
            Map(x => x.Gender).Not.Nullable();
            Map(x => x.Skin).Not.Nullable();
            Map(x => x.Face).Not.Nullable();
            Map(x => x.HairStyle).Not.Nullable();
            Map(x => x.HairColor).Not.Nullable();
            Map(x => x.FacialHair).Not.Nullable();
            Map(x => x.Outfit).Not.Nullable();
            Map(x => x.Level).Not.Nullable();
            Map(x => x.Xp);
            Map(x => x.TotalPlayTime).Not.Nullable();
            Map(x => x.LevelPlayTime).Not.Nullable();
            Map(x => x.TutorialFlags).Length(32).Not.Nullable(); //BinaryBlob
            Map(x => x.ExploredZones).Not.Nullable(); //BinaryBlob
            Map(x => x.PositionX).Not.Nullable();
            Map(x => x.PositionY).Not.Nullable();
            Map(x => x.PositionZ).Not.Nullable();
            Map(x => x.Orientation).Not.Nullable();
            Map(x => x.CorpseX).Nullable();
            Map(x => x.CorpseY);
            Map(x => x.CorpseZ);
            Map(x => x.CorpseO);
            Map(x => x.BindX).Not.Nullable();
            Map(x => x.BindY).Not.Nullable();
            Map(x => x.BindZ).Not.Nullable();
            Map(x => x.RuneSetMask);
            Map(x => x.RuneCooldowns);
            Map(x => x.BaseStrength).Not.Nullable();
            Map(x => x.BaseStamina).Not.Nullable();
            Map(x => x.BaseSpirit).Not.Nullable();
            Map(x => x.BaseIntellect).Not.Nullable();
            Map(x => x.BaseAgility).Not.Nullable();
            Map(x => x.GodMode);
            Map(x => x.Health).Not.Nullable();
            Map(x => x.BaseHealth).Not.Nullable();
            Map(x => x.Power).Not.Nullable();
            Map(x => x.BasePower).Not.Nullable();
            Map(x => x.Money).Not.Nullable();
            Map(x => x.FinishedQuests);
            Map(x => x.FinishedDailyQuests);
            Map(x => x.GuildId);
            Map(x => x.RestXp);
            Map(x => x.RestTriggerId);
            Map(x => x.NextTaxiVertexId);
            Map(x => x.TaxiMask);
            Map(x => x.PetSummonSpellId);
            Map(x => x.PetEntryId);
            Map(x => x.IsPetActive);
            Map(x => x.StableSlotCount);
            Map(x => x.PetSummonedCount);
            Map(x => x.PetCount);
            Map(x => x.PetHealth);
            Map(x => x.PetPower);
            Map(x => x.PetDuration);
            Map(x => x.LastTalentResetTime).Nullable();
            Map(Reveal.Member<CharacterRecord>("_talentResetPriceTier")).Not.Nullable();
            Map(x => x.DungeonDifficulty);
            Map(x => x.RaidDifficulty);
            Map(x => x.BattlegroundTeam).Default(BattlegroundSide.End.ToString());
            Map(x => x.KillsTotal);
            Map(x => x.HonorToday);
            Map(x => x.HonorYesterday);
            Map(x => x.LifetimeHonorableKills);
            Map(x => x.HonorPoints);
            Map(x => x.ArenaPoints);
        }
    }
}
