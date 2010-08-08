using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.Lang
{
	/// <summary>
	/// Keys for localizable strings
	/// </summary>
	public enum RealmLangKey
	{
		// ########################################################################
		// Misc
		None = 0,
		Done,
		Custom,
		Addon,
		Library,
		NotConnectedToAuthServer,
		PlayerNotOnline,
		NoValidTarget,
		InvalidSelection,

		// #######################################################################
		// Commands
		SubCommandNotFound,
		MustNotUseCommand,

		CmdSummonPlayerNotOnline,

		CmdKickMustProvideName,

		CmdLocalizerDescription,
		CmdLocalizerReloadDescription,
		CmdLocalizerSetLocaleDescription,
		CmdLocalizerSetLocaleParamInfo,
		LocaleSet,
		UnableToSetUserLocale,

        // #######################################################################
        // Spell Commands
        CmdSpellDescription,
        CmdSpellAddDescription,
        CmdSpellAddParamInfo,
        CmdSpellAddResponseSpell,
        CmdSpellAddResponseSpells,
        CmdSpellAddResponseTalents,
        CmdSpellRemoveDescription,
        CmdSpellRemoveParamInfo,
        CmdSpellRemoveResponse,
        CmdSpellRemoveError,
        CmdSpellPurgeDescription,
        CmdSpellPurgeResponse,
        CmdSpellPurgeError,
        CmdSpellTriggerDescription,
        CmdSpellTriggerParamInfo,
        CmdSpellTriggerResponse,
        CmdSpellTriggerError,
		CmdSpellGetDescription,
		CmdSpellGetParamInfo,
        CmdSpellClearDescription,
        CmdSpellClearResponse,
        CmdSpellClearError,
        CmdSpellVisualDescription,
        CmdSpellVisualParamInfo,
        CmdSpellVisualError,
        CmdSpellNotExists,

		CmdPushbackParams,
		CmdPushbackDescription,

        // ########################################################################
        // Skill Commands
        CmdSkillDescription,
        CmdSkillSetParamInfo,
        CmdSkillSetDescription,
        CmdSkillSetResponse,
        CmdSkillSetError,
        CmdSkillLearnParamInfo,
        CmdSkillLearnDescription,
        CmdSkillLearnResponse,
        CmdSkillLearnError,
        CmdSkillTierParamInfo,
        CmdSkillTierDescription,
        CmdSkillTierResponse,
        CmdSkillTierError1,
        CmdSkillTierError2,

        // ########################################################################
        // Region Commands
        CmdRegionDescription,
        CmdRegionSpawnParamInfo,
        CmdRegionSpawnDescription,
        CmdRegionSpawnResponse,
        CmdRegionSpawnResponse1,
        CmdRegionSpawnError1,
        CmdRegionSpawnError2,
        CmdRegionSpawnError3,
        CmdRegionSpawnResponse2,
        CmdRegionSpawnError4,
        CmdRegionSpawnError5,
        CmdRegionSpawnResponse3,
        CmdRegionClearParamInfo,
        CmdRegionClearDescription,
        CmdRegionClearError1,
        CmdRegionClearError2,
        CmdRegionClearResponse,
        CmdRegionUpdateDescription,
        CmdRegionListDescription,
        CmdRegionListResponse,

		// ########################################################################
		// Gossips
		GossipOptionBanker,
		GossipOptionFlightMaster,
		GossipOptionTrainer,
		GossipOptionVendor,
        GossipOptionInnKeeper,
        GossipOptionSpiritHealer,
        GossipOptionTabardDesigner,
        GossipOptionStableMaster,

		// ########################################################################
		// Ingame notifications
		GodModeIsActivated,

        // ########################################################################
        // Error notifications
        FeatureNotYetImplemented,
        InvalidClass
	}
}