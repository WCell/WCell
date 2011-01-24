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
            neutralAuraSpawn = GOMgr.GetEntry(GOEntryId.NeutralBannerAura).Templates[(int)ArathiBases.Farm];

            allianceBannerSpawn = GOMgr.GetEntry(GOEntryId.AllianceBanner_10).Templates[(int)ArathiBases.Farm];
            allianceAuraSpawn = GOMgr.GetEntry(GOEntryId.AllianceBannerAura).Templates[(int)ArathiBases.Farm];

            hordeBannerSpawn = GOMgr.GetEntry(GOEntryId.HordeBanner_10).Templates[(int)ArathiBases.Farm];
            hordeAuraSpawn = GOMgr.GetEntry(GOEntryId.HordeBannerAura).Templates[(int)ArathiBases.Farm];

            allianceAttackBannerSpawn = GOMgr.GetEntry(GOEntryId.ContestedBanner_26).Templates[(int)ArathiBases.Farm];
            hordeAttackBannerSpawn = GOMgr.GetEntry(GOEntryId.ContestedBanner_25).Templates[(int)ArathiBases.Farm];
        }

        public override string BaseName
        {
            get { return "Farm"; }
        }
    }
}