using WCell.Constants;
using WCell.Constants.Battlegrounds;
using WCell.Constants.GameObjects;
using WCell.Constants.World;
using WCell.Addons.Default.Lang;
using WCell.RealmServer.GameObjects;

namespace WCell.Addons.Default.Battlegrounds.ArathiBasin.Bases
{
    class Farm : ArathiBase
    {
        public Farm(ArathiBasin instance)
            : base(instance)
        {
            showIconNeutral = WorldStateId.ABShowFarmIcon;
            showIconAllianceContested = WorldStateId.ABShowFarmIconAllianceContested;
            showIconAllianceControlled = WorldStateId.ABShowFarmIconAlliance;
            showIconHordeContested = WorldStateId.ABShowFarmIconHordeContested;
            showIconHordeControlled = WorldStateId.ABShowFarmIconHorde;

            Names = DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ABFarm);
        }

        protected override void AddSpawns()
        {
            neutralBannerSpawn = GOMgr.GetEntry(GOEntryId.FarmBanner_2).FirstSpawnEntry;
            neutralAuraSpawn = GOMgr.GetEntry(GOEntryId.NeutralBannerAura).SpawnEntries[(int)ArathiBases.Farm];

            allianceBannerSpawn = GOMgr.GetEntry(GOEntryId.AllianceBanner_10).SpawnEntries[(int)ArathiBases.Farm];
            allianceAuraSpawn = GOMgr.GetEntry(GOEntryId.AllianceBannerAura).SpawnEntries[(int)ArathiBases.Farm];

            hordeBannerSpawn = GOMgr.GetEntry(GOEntryId.HordeBanner_10).SpawnEntries[(int)ArathiBases.Farm];
            hordeAuraSpawn = GOMgr.GetEntry(GOEntryId.HordeBannerAura).SpawnEntries[(int)ArathiBases.Farm];

            allianceAttackBannerSpawn = GOMgr.GetEntry(GOEntryId.ContestedBanner_26).SpawnEntries[(int)ArathiBases.Farm];
            hordeAttackBannerSpawn = GOMgr.GetEntry(GOEntryId.ContestedBanner_25).SpawnEntries[(int)ArathiBases.Farm];
        }

        public override string BaseName
        {
            get { return "Farm"; }
        }
    }
}