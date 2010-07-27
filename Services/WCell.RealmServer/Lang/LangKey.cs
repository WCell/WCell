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

		CmdSpellGetDescription,
		CmdSpellGetParamInfo,


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
        FeatureNotYetImplemented
	}
}