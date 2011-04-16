using WCell.Constants;
using WCell.Constants.Battlegrounds;
using WCell.Constants.GameObjects;
using WCell.Constants.World;
using WCell.Addons.Default.Lang;
using WCell.RealmServer.GameObjects;

namespace WCell.Addons.Default.Battlegrounds.ArathiBasin.Bases
{
    class Blacksmith : ArathiBase
    {
        public Blacksmith(ArathiBasin instance) 
            : base(instance)
        {
            showIconNeutral = WorldStateId.ABShowBlacksmithIcon;
            showIconAllianceContested = WorldStateId.ABShowBlacksmithIconAllianceContested;
            showIconAllianceControlled = WorldStateId.ABShowBlacksmithIconAlliance;
            showIconHordeContested = WorldStateId.ABShowBlacksmithIconHordeContested;
            showIconHordeControlled = WorldStateId.ABShowBlacksmithIconHorde;

            Names = DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ABBlacksmith);
        }

        protected override void AddSpawns()
        {
            neutralBannerSpawn = GOMgr.GetEntry(GOEntryId.BlacksmithBanner_2).FirstSpawnEntry;
            neutralAuraSpawn = GOMgr.GetEntry(GOEntryId.NeutralBannerAura).SpawnEntries[(int)ArathiBases.Blacksmith];

            allianceBannerSpawn = GOMgr.GetEntry(GOEntryId.AllianceBanner_10).SpawnEntries[(int)ArathiBases.Blacksmith];
            allianceAuraSpawn = GOMgr.GetEntry(GOEntryId.AllianceBannerAura).SpawnEntries[(int)ArathiBases.Blacksmith];

            hordeBannerSpawn = GOMgr.GetEntry(GOEntryId.HordeBanner_10).SpawnEntries[(int)ArathiBases.Blacksmith];
            hordeAuraSpawn = GOMgr.GetEntry(GOEntryId.HordeBannerAura).SpawnEntries[(int)ArathiBases.Blacksmith];

            allianceAttackBannerSpawn = GOMgr.GetEntry(GOEntryId.ContestedBanner_26).SpawnEntries[(int)ArathiBases.Blacksmith];
            hordeAttackBannerSpawn = GOMgr.GetEntry(GOEntryId.ContestedBanner_25).SpawnEntries[(int)ArathiBases.Blacksmith];
        }

        public override string BaseName
        {
            get { return "Blacksmith"; }
        }
    }
}