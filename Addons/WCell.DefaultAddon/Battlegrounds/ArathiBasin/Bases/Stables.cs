using WCell.Constants;
using WCell.Constants.Battlegrounds;
using WCell.Constants.GameObjects;
using WCell.Constants.World;
using WCell.Addons.Default.Lang;
using WCell.RealmServer.GameObjects;

namespace WCell.Addons.Default.Battlegrounds.ArathiBasin
{
    class Stables : ArathiBase
    {
        public Stables(ArathiBasin instance)
            : base(instance)
        {
            showIconNeutral = WorldStateId.ABShowStableIcon;
            showIconAllianceContested = WorldStateId.ABShowStableIconAllianceContested;
            showIconAllianceControlled = WorldStateId.ABShowStableIconAlliance;
            showIconHordeContested = WorldStateId.ABShowStableIconHordeContested;
            showIconHordeControlled = WorldStateId.ABShowStableIconHorde;

            Names = DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ABStables);
        }

        protected override void AddSpawns()
        {
            neutralBannerSpawn = GOMgr.GetEntry(GOEntryId.StableBanner_2).FirstSpawnEntry;
            neutralAuraSpawn = GOMgr.GetEntry(GOEntryId.NeutralBannerAura).Templates[(int)ArathiBases.Stables];

            allianceBannerSpawn = GOMgr.GetEntry(GOEntryId.AllianceBanner_10).Templates[(int)ArathiBases.Stables];
            allianceAuraSpawn = GOMgr.GetEntry(GOEntryId.AllianceBannerAura).Templates[(int)ArathiBases.Stables];

            hordeBannerSpawn = GOMgr.GetEntry(GOEntryId.HordeBanner_10).Templates[(int)ArathiBases.Stables];
            hordeAuraSpawn = GOMgr.GetEntry(GOEntryId.HordeBannerAura).Templates[(int)ArathiBases.Stables];

            allianceAttackBannerSpawn = GOMgr.GetEntry(GOEntryId.ContestedBanner_26).Templates[(int)ArathiBases.Stables];
            hordeAttackBannerSpawn = GOMgr.GetEntry(GOEntryId.ContestedBanner_25).Templates[(int)ArathiBases.Stables];
        }

        public override string BaseName
        {
            get { return "Stables"; }
        }
    }
}