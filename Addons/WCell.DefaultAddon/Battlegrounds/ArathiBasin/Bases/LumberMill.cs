using WCell.Addons.Default.Lang;
using WCell.Constants.Battlegrounds;
using WCell.Constants.GameObjects;
using WCell.Constants.World;
using WCell.RealmServer.GameObjects;

namespace WCell.Addons.Default.Battlegrounds.ArathiBasin.Bases
{
    class LumberMill : ArathiBase
    {
        public LumberMill(ArathiBasin instance)
            : base(instance)
        {
            showIconNeutral = WorldStateId.ABShowLumberMillIcon;
            showIconAllianceContested = WorldStateId.ABShowLumberMillIconAllianceContested;
            showIconAllianceControlled = WorldStateId.ABShowLumberMillIconAlliance;
            showIconHordeContested = WorldStateId.ABShowLumberMillIconHordeContested;
            showIconHordeControlled = WorldStateId.ABShowLumberMillIconHorde;

            Names = DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ABLumberMill);
        }

        protected override void AddSpawns()
        {
            neutralBannerSpawn = GOMgr.GetEntry(GOEntryId.LumberMillBanner_2).FirstSpawnEntry;
            neutralAuraSpawn = GOMgr.GetEntry(GOEntryId.NeutralBannerAura).SpawnEntries[(int)ArathiBases.Lumbermill];

            allianceBannerSpawn = GOMgr.GetEntry(GOEntryId.AllianceBanner_10).SpawnEntries[(int)ArathiBases.Lumbermill];
            allianceAuraSpawn = GOMgr.GetEntry(GOEntryId.AllianceBannerAura).SpawnEntries[(int)ArathiBases.Lumbermill];

            hordeBannerSpawn = GOMgr.GetEntry(GOEntryId.HordeBanner_10).SpawnEntries[(int)ArathiBases.Lumbermill];
            hordeAuraSpawn = GOMgr.GetEntry(GOEntryId.HordeBannerAura).SpawnEntries[(int)ArathiBases.Lumbermill];

            allianceAttackBannerSpawn = GOMgr.GetEntry(GOEntryId.ContestedBanner_26).SpawnEntries[(int)ArathiBases.Lumbermill];
            hordeAttackBannerSpawn = GOMgr.GetEntry(GOEntryId.ContestedBanner_25).SpawnEntries[(int)ArathiBases.Lumbermill];

        }
        public override string BaseName
        {
            get { return "Lumber Mill"; }
        }
    }
}