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
		None = 0,
		Addon,
		Library,
		Done,

		// #######################################################################
		// Commands
		SubCommandNotFound,
		MustNotUseCommand,

		CmdLocalizerDescription,
		CmdLocalizerReloadDescription,
		CmdLocalizerSetLocaleDescription,
		CmdLocalizerSetLocaleParamInfo,
		LocaleSet,
		UnableToSetUserLocale
	}

	public class TranslatableItem : Util.Lang.TranslatableItem<LangKey>
	{
		public TranslatableItem(LangKey key, params object[] args) : base(key, args)
		{
		}
	}
}
