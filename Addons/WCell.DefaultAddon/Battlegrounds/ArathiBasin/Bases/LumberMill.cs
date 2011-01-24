using WCell.Constants;
using WCell.Constants.Battlegrounds;
using WCell.Constants.GameObjects;
using WCell.Constants.World;
using WCell.Addons.Default.Lang;
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
            neutralAuraSpawn = GOMgr.GetEntry(GOEntryId.NeutralBannerAura).Templates[(int)ArathiBases.Lumbermill];

            allianceBannerSpawn = GOMgr.GetEntry(GOEntryId.AllianceBanner_10).Templates[(int)ArathiBases.Lumbermill];
            allianceAuraSpawn = GOMgr.GetEntry(GOEntryId.AllianceBannerAura).Templates[(int)ArathiBases.Lumbermill];

            hordeBannerSpawn = GOMgr.GetEntry(GOEntryId.HordeBanner_10).Templates[(int)ArathiBases.Lumbermill];
            hordeAuraSpawn = GOMgr.GetEntry(GOEntryId.HordeBannerAura).Templates[(int)ArathiBases.Lumbermill];

            allianceAttackBannerSpawn = GOMgr.GetEntry(GOEntryId.ContestedBanner_26).Templates[(int)ArathiBases.Lumbermill];
            hordeAttackBannerSpawn = GOMgr.GetEntry(GOEntryId.ContestedBanner_25).Templates[(int)ArathiBases.Lumbermill];
        }
        public override string BaseName
        {
            get { return "Lumber Mill"; }
        }
    }
}