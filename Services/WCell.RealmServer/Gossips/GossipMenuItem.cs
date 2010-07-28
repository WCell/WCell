using WCell.Constants;
using WCell.RealmServer.Lang;

namespace WCell.RealmServer.Gossips
{
	#region LocalizeGossipMenuItem
	public class LocalizedGossipMenuItem : GossipMenuItemBase
	{
		public readonly string[] Texts = new string[(int)ClientLocale.End];

		/// <summary>
		/// If set, will show an Accept/Cancel dialog with this text to the player
		/// when selecting this Item.
		/// </summary>
		public string[] ConfirmTexts = new string[(int)ClientLocale.End];


		public LocalizedGossipMenuItem()
		{
		}

		public LocalizedGossipMenuItem(GossipMenuIcon type, string[] texts)
		{
			Icon = type;
			Texts = texts;
		}

		public LocalizedGossipMenuItem(string[] texts)
			: this(GossipMenuIcon.Talk, texts)
		{
		}

		public LocalizedGossipMenuItem(string[] texts, IGossipAction action)
			: this(texts)
		{
			Action = action;
		}

		public LocalizedGossipMenuItem(string[] texts, GossipActionHandler callback)
			: this(texts)
		{
			Action = new DefaultGossipAction(callback);
		}

		public LocalizedGossipMenuItem(string[] texts, GossipActionHandler callback, string[] confirmTexts)
			: this(texts)
		{
			ConfirmTexts = confirmTexts;
			Action = new DefaultGossipAction(callback);
		}

		public LocalizedGossipMenuItem(string[] texts, GossipActionHandler callback, params LocalizedGossipMenuItem[] items)
			: this(texts)
		{
			Action = new DefaultGossipAction(callback);
			SubMenu = new GossipMenu(items);
		}

		public LocalizedGossipMenuItem(string[] texts, GossipMenu subMenu)
			: this(texts, (IGossipAction)null, subMenu)
		{
		}

		public LocalizedGossipMenuItem(string[] texts, GossipActionHandler callback, GossipMenu subMenu)
			: this(texts)
		{
			Action = new DefaultGossipAction(callback);
			SubMenu = subMenu;
		}

		public LocalizedGossipMenuItem(string[] texts, IGossipAction action, GossipMenu subMenu)
			: this(texts)
		{
			Action = action;
			SubMenu = subMenu;
		}

		public LocalizedGossipMenuItem(string[] texts, params LocalizedGossipMenuItem[] items)
			: this(texts)
		{
			SubMenu = new GossipMenu(items);
		}

		public LocalizedGossipMenuItem(GossipMenuIcon icon, string[] texts, params LocalizedGossipMenuItem[] items)
			: this(texts)
		{
			Icon = icon;
			SubMenu = new GossipMenu(items);
		}

		public LocalizedGossipMenuItem(GossipMenuIcon icon, string[] texts, IGossipAction action)
			: this(texts)
		{
			Icon = icon;
			Action = action;
		}

		public LocalizedGossipMenuItem(GossipMenuIcon icon, string[] texts, GossipActionHandler callback)
			: this(texts)
		{
			Icon = icon;
			Action = new DefaultGossipAction(callback);
		}

		public LocalizedGossipMenuItem(GossipMenuIcon type, LangKey langKey)
		{
			Icon = type;
			Texts = RealmLocalizer.Instance.GetTranslations(langKey);
		}

		public LocalizedGossipMenuItem(LangKey langKey)
			: this(GossipMenuIcon.Talk, langKey)
		{
		}

		public LocalizedGossipMenuItem(LangKey langKey, IGossipAction action)
			: this(langKey)
		{
			Action = action;
		}

		public LocalizedGossipMenuItem(LangKey langKey, GossipActionHandler callback)
			: this(langKey)
		{
			Action = new DefaultGossipAction(callback);
		}

		public LocalizedGossipMenuItem(LangKey langKey, GossipActionHandler callback, LangKey confirmLangKey)
			: this(langKey)
		{
			ConfirmTexts = RealmLocalizer.Instance.GetTranslations(confirmLangKey);
			Action = new DefaultGossipAction(callback);
		}

		public LocalizedGossipMenuItem(LangKey langKey, GossipActionHandler callback, params LocalizedGossipMenuItem[] items)
			: this(langKey)
		{
			Action = new DefaultGossipAction(callback);
			SubMenu = new GossipMenu(items);
		}

		public LocalizedGossipMenuItem(LangKey langKey, GossipMenu subMenu)
			: this(langKey, (IGossipAction)null, subMenu)
		{
		}

		public LocalizedGossipMenuItem(LangKey langKey, GossipActionHandler callback, GossipMenu subMenu)
			: this(langKey)
		{
			Action = new DefaultGossipAction(callback);
			SubMenu = subMenu;
		}

		public LocalizedGossipMenuItem(LangKey langKey, IGossipAction action, GossipMenu subMenu)
			: this(langKey)
		{
			Action = action;
			SubMenu = subMenu;
		}

		public LocalizedGossipMenuItem(LangKey langKey, params LocalizedGossipMenuItem[] items)
			: this(langKey)
		{
			SubMenu = new GossipMenu(items);
		}

		public LocalizedGossipMenuItem(GossipMenuIcon icon, LangKey langKey, params LocalizedGossipMenuItem[] items)
			: this(langKey)
		{
			Icon = icon;
			SubMenu = new GossipMenu(items);
		}

		public LocalizedGossipMenuItem(GossipMenuIcon icon, LangKey langKey, IGossipAction action)
			: this(langKey)
		{
			Icon = icon;
			Action = action;
		}

		public LocalizedGossipMenuItem(GossipMenuIcon icon, LangKey langKey, GossipActionHandler callback)
			: this(langKey)
		{
			Icon = icon;
			Action = new DefaultGossipAction(callback);
		}

		public string DefaultText
		{
			get { return Texts.LocalizeWithDefaultLocale(); }
			set { Texts[(int)RealmServerConfiguration.DefaultLocale] = value; }
		}

		public string DefaultConfirmText
		{
			get { return ConfirmTexts.LocalizeWithDefaultLocale(); }
			set { ConfirmTexts[(int)RealmServerConfiguration.DefaultLocale] = value; }
		}

		public override string GetText(GossipConversation convo)
		{
			return Texts.Localize(convo.User.Locale);
		}

		public override string GetConfirmText(GossipConversation convo)
		{
			return ConfirmTexts.Localize(convo.User.Locale);
		}
	}
	#endregion

	#region GossipMenuItem
	public class GossipMenuItem : GossipMenuItemBase
	{
		public string Text;

		/// <summary>
		/// If set, will show an Accept/Cancel dialog with this text to the player
		/// when selecting this Item.
		/// </summary>
		public string ConfirmText;

		public GossipMenuItem()
		{
			Text = string.Empty;
			ConfirmText = string.Empty;
		}

		public GossipMenuItem(GossipMenuIcon type, string text)
		{
			Icon = type;
			Text = text;
			ConfirmText = string.Empty;
		}

		public GossipMenuItem(string text)
			: this(GossipMenuIcon.Talk, text)
		{
		}

		public GossipMenuItem(string text, IGossipAction action)
			: this(text)
		{
			Action = action;
		}

		public GossipMenuItem(string text, GossipActionHandler callback)
			: this(text)
		{
			Action = new DefaultGossipAction(callback);
		}

		public GossipMenuItem(string text, GossipActionHandler callback, string confirmText)
			: this(text)
		{
			ConfirmText = confirmText;
			Action = new DefaultGossipAction(callback);
		}

		public GossipMenuItem(string text, GossipActionHandler callback, params GossipMenuItem[] items)
			: this(text)
		{
			Action = new DefaultGossipAction(callback);
			SubMenu = new GossipMenu(items);
		}

		public GossipMenuItem(string text, GossipMenu subMenu)
			: this(text, (IGossipAction)null, subMenu)
		{
		}

		public GossipMenuItem(string text, GossipActionHandler callback, GossipMenu subMenu)
			: this(text)
		{
			Action = new DefaultGossipAction(callback);
			SubMenu = subMenu;
		}

		public GossipMenuItem(string text, IGossipAction action, GossipMenu subMenu)
			: this(text)
		{
			Action = action;
			SubMenu = subMenu;
		}

		public GossipMenuItem(string text, params GossipMenuItem[] items)
			: this(text)
		{
			SubMenu = new GossipMenu(items);
		}

		public GossipMenuItem(GossipMenuIcon icon, string text, params GossipMenuItem[] items)
			: this(text)
		{
			Icon = icon;
			SubMenu = new GossipMenu(items);
		}

		public GossipMenuItem(GossipMenuIcon icon, string text, IGossipAction action)
			: this(text)
		{
			Icon = icon;
			Action = action;
		}

		public GossipMenuItem(GossipMenuIcon icon, string text, GossipActionHandler callback)
			: this(text)
		{
			Icon = icon;
			Action = new DefaultGossipAction(callback);
		}

		public override string GetText(GossipConversation convo)
		{
			return Text;
		}

		public override string GetConfirmText(GossipConversation convo)
		{
			return ConfirmText;
		}
	}
	#endregion

	#region QuitGossipMenuItem
	public class QuitGossipMenuItem : GossipMenuItem
	{
		public QuitGossipMenuItem(GossipMenuIcon type, string text)
			: base(type, text)
		{
		}

		public QuitGossipMenuItem(string text)
			: base(text)
		{
			Action = new DefaultGossipAction((convo) => {
				convo.Character.GossipConversation.StayOpen = false;
			});
		}

		public QuitGossipMenuItem(string text, GossipActionHandler callback) :
			base(text)
		{
			Action = new DefaultGossipAction((convo) => {
				convo.Character.GossipConversation.StayOpen = false;
				callback(convo);
			});
		}

		public QuitGossipMenuItem(string text, GossipActionHandler callback, string confirmText)
			: base(text)
		{
			ConfirmText = confirmText;
			Action = new DefaultGossipAction((convo) => {
				convo.Character.GossipConversation.StayOpen = false;
				callback(convo);
			});
		}

		public QuitGossipMenuItem(string text, GossipActionHandler callback, params GossipMenuItem[] items)
			: base(text, items)
		{
			Action = new DefaultGossipAction((convo) => {
				convo.Character.GossipConversation.StayOpen = false;
				callback(convo);
			});
		}

		public QuitGossipMenuItem(string text, GossipMenu subMenu)
			: base(text, subMenu)
		{
			Action = new DefaultGossipAction((convo) => {
				convo.Character.GossipConversation.StayOpen = false;
			});
		}

		public QuitGossipMenuItem(string text, GossipActionHandler callback, GossipMenu subMenu)
			: base(text, subMenu)
		{
			Action = new DefaultGossipAction((convo) => {
				convo.Character.GossipConversation.StayOpen = false;
				callback(convo);
			});
		}

		public QuitGossipMenuItem(string text, params GossipMenuItem[] items)
			: base(text, items)
		{
			Action = new DefaultGossipAction((convo) => {
				convo.Character.GossipConversation.StayOpen = false;
			});
		}

		public QuitGossipMenuItem(GossipMenuIcon icon, string text, params GossipMenuItem[] items)
			: base(icon, text, items)
		{
			Action = new DefaultGossipAction((convo) => {
				convo.Character.GossipConversation.StayOpen = false;
			});
		}

		public QuitGossipMenuItem(GossipMenuIcon icon, string text, GossipActionHandler callback)
			: base(icon, text)
		{
			Action = new DefaultGossipAction((convo) => {
				convo.Character.GossipConversation.StayOpen = false;
				callback(convo);
			});
		}
	}
	#endregion

	/// <summary>
	/// Represents action (battlemaster, flightmaster, etc.) gossip item in menu
	/// </summary>
	public abstract class GossipMenuItemBase
	{
		public GossipMenuIcon Icon;
		public int RequiredMoney;
		public byte Input;

		/// <summary>
		/// Determines if character is elligible for viewing this item and action taken on item selection
		/// </summary>
		public IGossipAction Action;

		/// <summary>
		/// The <see cref="GossipMenu"/> to be shown when selecting this Item
		/// </summary>
		public GossipMenu SubMenu
		{
			get;
			set;
		}

		public void SetAction(DefaultGossipAction action)
		{
			Action = action;
		}

		public abstract string GetText(GossipConversation convo);

		public abstract string GetConfirmText(GossipConversation convo);
	}

	/// <summary>
	/// Represents quest item in menu
	/// </summary>
	public class QuestMenuItem
	{
		public uint ID;
		public uint Status;
		public uint Level;
		public string Text;

		public QuestMenuItem()
		{
			Text = string.Empty;
		}

		public QuestMenuItem(uint id, uint status, uint level, string text)
		{
			ID = id;
			Status = status;
			Text = text;
			Level = level;
		}
	}
}