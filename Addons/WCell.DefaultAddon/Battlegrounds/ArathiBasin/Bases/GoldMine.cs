using WCell.Constants;
using WCell.Constants.Battlegrounds;
using WCell.Constants.GameObjects;
using WCell.Constants.World;
using WCell.Addons.Default.Lang;
using WCell.RealmServer.GameObjects;

namespace WCell.Addons.Default.Battlegrounds.ArathiBasin.Bases
{
    class GoldMine : ArathiBase
    {
        public GoldMine(ArathiBasin instance)
            : base(instance)
        {
            showIconNeutral = WorldStateId.ABShowGoldMineIcon;
            showIconAllianceContested = WorldStateId.ABShowGoldMineIconAllianceContested;
            showIconAllianceControlled = WorldStateId.ABShowGoldMineIconAlliance;
            showIconHordeContested = WorldStateId.ABShowGoldMineIconHordeContested;
            showIconHordeControlled = WorldStateId.ABShowGoldMineIconHorde;

            Names = DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ABGoldMine);
        }

        protected override void AddSpawns()
        {
            neutralBannerSpawn = GOMgr.GetEntry(GOEntryId.MineBanner_2).FirstSpawnEntry;
            neutralAuraSpawn = GOMgr.GetEntry(GOEntryId.NeutralBannerAura).Templates[(int)ArathiBases.GoldMine];

            allianceBannerSpawn = GOMgr.GetEntry(GOEntryId.AllianceBanner_10).Templates[(int)ArathiBases.GoldMine];
            allianceAuraSpawn = GOMgr.GetEntry(GOEntryId.AllianceBannerAura).Templates[(int)ArathiBases.GoldMine];

            hordeBannerSpawn = GOMgr.GetEntry(GOEntryId.HordeBanner_10).Templates[(int)ArathiBases.GoldMine];
            hordeAuraSpawn = GOMgr.GetEntry(GOEntryId.HordeBannerAura).Templates[(int)ArathiBases.GoldMine];

            allianceAttackBannerSpawn = GOMgr.GetEntry(GOEntryId.ContestedBanner_26).Templates[(int)ArathiBases.GoldMine];
            hordeAttackBannerSpawn = GOMgr.GetEntry(GOEntryId.ContestedBanner_25).Templates[(int)ArathiBases.GoldMine];
        }

        public override string BaseName
        {
            get { return "Gold Mine"; }
        }
    }
}