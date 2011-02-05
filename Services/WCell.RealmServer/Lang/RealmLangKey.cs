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
		PleaseWait,
		GoBack,
		Custom,
		AreYouSure,

		Addon,
		Library,
		NotConnectedToAuthServer,
		PlayerNotOnline,
		NoValidTarget,
		InvalidSelection,

		// ########################################################################
		// Time
		Second,
		Seconds,
		Minute,
		Minutes,
		Hour,
		Hours,

		// ########################################################################
		// Ingame messages
		LogCombatExp,
		LogCombatExpRested,

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
        // Map Commands
        CmdMapDescription,
        CmdMapSpawnParamInfo,
        CmdMapSpawnDescription,
        CmdMapSpawnResponse,
        CmdMapSpawnResponse1,
        CmdMapSpawnError1,
        CmdMapSpawnError2,
        CmdMapSpawnError3,
        CmdMapSpawnResponse2,
        CmdMapSpawnError4,
        CmdMapSpawnError5,
        CmdMapSpawnResponse3,
        CmdMapClearParamInfo,
        CmdMapClearDescription,
        CmdMapClearError1,
        CmdMapClearError2,
        CmdMapClearResponse,
        CmdMapUpdateDescription,
        CmdMapListDescription,
        CmdMapListResponse,

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
        InvalidClass,

		// Other
		Goodbye,

		// ########################################################################
		// Editor
		EditorMapMenuLoadData,
		EditorMapMenuSpawnMap,
		EditorMapMenuClearMap,
		EditorMapMenuShow,
		EditorMapMenuHide,
		EditorMapMenuEnableAllSpawnPoints,
		EditorMapMenuDisableAllSpawnPoints,

		EditorSpawnPointMenuMoveOverHere
	}
}