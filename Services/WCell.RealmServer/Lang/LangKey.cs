using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.Lang
{
	/// <summary>
	/// Keys for strings used in commands
	/// </summary>
	public enum LangKey
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

	public class TranslatableItem : Util.Lang.TranslatableItem<LangKey>
	{
		public TranslatableItem(LangKey key, params object[] args) : base(key, args)
		{
		}
	}
}