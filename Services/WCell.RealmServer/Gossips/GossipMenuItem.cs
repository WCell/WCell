using WCell.Constants;
using WCell.RealmServer.Lang;

namespace WCell.RealmServer.Gossips
{
	#region LocalizeGossipMenuItem
	public class LocalizedGossipMenuItem : GossipMenuItemBase
	{
		public readonly TranslatableItem Text;

		/// <summary>
		/// If set, will show an Accept/Cancel dialog with this text to the player
		/// when selecting this Item.
		/// </summary>
		public TranslatableItem ConfirmText;


		public LocalizedGossipMenuItem()
		{
		}

		public LocalizedGossipMenuItem(GossipMenuIcon type, TranslatableItem text)
		{
			Icon = type;
			Text = text;
		}

		public LocalizedGossipMenuItem(TranslatableItem text)
			: this(GossipMenuIcon.Talk, text)
		{
		}

		public LocalizedGossipMenuItem(RealmLangKey msgKey)
		{
			Text = new TranslatableItem(msgKey);
		}

		public LocalizedGossipMenuItem(TranslatableItem text, IGossipAction action)
			: this(text)
		{
			Action = action;
		}

		public LocalizedGossipMenuItem(TranslatableItem text, GossipActionHandler callback)
			: this(text)
		{
			Action = new NonNavigatingGossipAction(callback);
		}

		public LocalizedGossipMenuItem(TranslatableItem text, GossipActionHandler callback, TranslatableItem confirmText)
			: this(text)
		{
			ConfirmText = confirmText;
			Action = new NonNavigatingGossipAction(callback);
		}

		public LocalizedGossipMenuItem(TranslatableItem text, GossipActionHandler callback, params LocalizedGossipMenuItem[] items)
			: this(text)
		{
			Action = new NonNavigatingGossipAction(callback);
			SubMenu = new GossipMenu(items);
		}

		public LocalizedGossipMenuItem(TranslatableItem text, GossipMenu subMenu)
			: this(text, (IGossipAction)null, subMenu)
		{
		}

		public LocalizedGossipMenuItem(TranslatableItem text, GossipActionHandler callback, GossipMenu subMenu)
			: this(text)
		{
			Action = new NonNavigatingGossipAction(callback);
			SubMenu = subMenu;
		}

		public LocalizedGossipMenuItem(TranslatableItem text, IGossipAction action, GossipMenu subMenu)
			: this(text)
		{
			Action = action;
			SubMenu = subMenu;
		}

		public LocalizedGossipMenuItem(TranslatableItem text, params LocalizedGossipMenuItem[] items)
			: this(text)
		{
			SubMenu = new GossipMenu(items);
		}

		public LocalizedGossipMenuItem(GossipMenuIcon icon, TranslatableItem text, params LocalizedGossipMenuItem[] items)
			: this(text)
		{
			Icon = icon;
			SubMenu = new GossipMenu(items);
		}

		public LocalizedGossipMenuItem(GossipMenuIcon icon, TranslatableItem text, IGossipAction action)
			: this(text)
		{
			Icon = icon;
			Action = action;
		}

		public LocalizedGossipMenuItem(GossipMenuIcon icon, TranslatableItem text, GossipActionHandler callback)
			: this(text)
		{
			Icon = icon;
			Action = new NonNavigatingGossipAction(callback);
		}

		public LocalizedGossipMenuItem(GossipMenuIcon icon, RealmLangKey msgKey, params object[] msgArgs)
			: this(msgKey, msgArgs)
		{
			Icon = icon;
		}

		public LocalizedGossipMenuItem(RealmLangKey msgKey, params object[] msgArgs)
			: this(GossipMenuIcon.Talk, new TranslatableItem(msgKey, msgArgs))
		{
		}

		public LocalizedGossipMenuItem(IGossipAction action, RealmLangKey msgKey, params object[] msgArgs)
			: this(msgKey, msgArgs)
		{
			Action = action;
		}

		public LocalizedGossipMenuItem(GossipActionHandler callback, RealmLangKey msgKey, params object[] msgArgs)
			: this(msgKey, msgArgs)
		{
			Action = new NonNavigatingGossipAction(callback);
		}

		public LocalizedGossipMenuItem(GossipActionHandler callback, RealmLangKey confirmLangKey, RealmLangKey msgKey, params object[] msgArgs)
			: this(msgKey, msgArgs)
		{
			ConfirmText = new TranslatableItem(confirmLangKey);
			Action = new NonNavigatingGossipAction(callback);
		}

		//public LocalizedGossipMenuItem(GossipActionHandler callback, RealmLangKey msgKey, params LocalizedGossipMenuItem[] items)
		//    : this(new TranslatableItem(msgKey))
		//{
		//    Action = new DefaultGossipAction(callback);
		//    SubMenu = new GossipMenu(items);
		//}

		public LocalizedGossipMenuItem(GossipMenu subMenu, RealmLangKey msgKey, params object[] msgArgs)
			: this(msgKey, msgArgs, (IGossipAction)null, subMenu)
		{
		}

		public LocalizedGossipMenuItem(GossipActionHandler callback, GossipMenu subMenu, RealmLangKey msgKey, params object[] msgArgs)
			: this(msgKey, msgArgs)
		{
			Action = new NonNavigatingGossipAction(callback);
			SubMenu = subMenu;
		}

		public LocalizedGossipMenuItem(IGossipAction action, GossipMenu subMenu, RealmLangKey msgKey, params object[] msgArgs)
			: this(msgKey, msgArgs)
		{
			Action = action;
			SubMenu = subMenu;
		}

		public LocalizedGossipMenuItem(RealmLangKey msgKey, params LocalizedGossipMenuItem[] items)
			: this(msgKey)
		{
			SubMenu = new GossipMenu(items);
		}

		public LocalizedGossipMenuItem(GossipMenuIcon icon, RealmLangKey langKey, params LocalizedGossipMenuItem[] items)
			: this(langKey)
		{
			Icon = icon;
			SubMenu = new GossipMenu(items);
		}

		public LocalizedGossipMenuItem(GossipMenuIcon icon, IGossipAction action, RealmLangKey msgKey, params object[] msgArgs)
			: this(msgKey, msgArgs)
		{
			Icon = icon;
			Action = action;
		}

		public LocalizedGossipMenuItem(GossipMenuIcon icon, GossipActionHandler callback, RealmLangKey msgKey, params object[] msgArgs)
			: this(msgKey, msgArgs)
		{
			Icon = icon;
			Action = new NonNavigatingGossipAction(callback);
		}

		public string DefaultText
		{
			get { return Text.TranslateDefault(); }
		}

		public string DefaultConfirmText
		{
			get { return ConfirmText.TranslateDefault(); }
		}

		public override string GetText(GossipConversation convo)
		{
			return Text.Translate(convo.User.Locale);
		}

		public override string GetConfirmText(GossipConversation convo)
		{
			return ConfirmText.Translate(convo.User.Locale);
		}
	}
	#endregion

	#region MultiStringGossipMenuItem
	public class MultiStringGossipMenuItem : GossipMenuItemBase
	{
		public readonly string[] Texts = new string[(int)ClientLocale.End];

		/// <summary>
		/// If set, will show an Accept/Cancel dialog with this text to the player
		/// when selecting this Item.
		/// </summary>
		public string[] ConfirmTexts = new string[(int)ClientLocale.End];


		public MultiStringGossipMenuItem()
		{
		}

		public MultiStringGossipMenuItem(GossipMenuIcon type, string[] texts)
		{
			Icon = type;
			Texts = texts;
		}

		public MultiStringGossipMenuItem(string[] texts)
			: this(GossipMenuIcon.Talk, texts)
		{
		}

		public MultiStringGossipMenuItem(string[] texts, IGossipAction action)
			: this(texts)
		{
			Action = action;
		}

		public MultiStringGossipMenuItem(string[] texts, GossipActionHandler callback)
			: this(texts)
		{
			Action = new NonNavigatingGossipAction(callback);
		}

		public MultiStringGossipMenuItem(string[] texts, GossipActionHandler callback, string[] confirmTexts)
			: this(texts)
		{
			ConfirmTexts = confirmTexts;
			Action = new NonNavigatingGossipAction(callback);
		}

		public MultiStringGossipMenuItem(string[] texts, GossipActionHandler callback, params MultiStringGossipMenuItem[] items)
			: this(texts)
		{
			Action = new NonNavigatingGossipAction(callback);
			SubMenu = new GossipMenu(items);
		}

		public MultiStringGossipMenuItem(string[] texts, GossipMenu subMenu)
			: this(texts, (IGossipAction)null, subMenu)
		{
		}

		public MultiStringGossipMenuItem(string[] texts, GossipActionHandler callback, GossipMenu subMenu)
			: this(texts)
		{
			Action = new NonNavigatingGossipAction(callback);
			SubMenu = subMenu;
		}

		public MultiStringGossipMenuItem(string[] texts, IGossipAction action, GossipMenu subMenu)
			: this(texts)
		{
			Action = action;
			SubMenu = subMenu;
		}

		public MultiStringGossipMenuItem(string[] texts, params MultiStringGossipMenuItem[] items)
			: this(texts)
		{
			SubMenu = new GossipMenu(items);
		}

		public MultiStringGossipMenuItem(GossipMenuIcon icon, string[] texts, params MultiStringGossipMenuItem[] items)
			: this(texts)
		{
			Icon = icon;
			SubMenu = new GossipMenu(items);
		}

		public MultiStringGossipMenuItem(GossipMenuIcon icon, string[] texts, GossipMenu subMenu)
			: this(texts)
		{
			Icon = icon;
			SubMenu = subMenu;
		}

		public MultiStringGossipMenuItem(GossipMenuIcon icon, string[] texts, IGossipAction action)
			: this(texts)
		{
			Icon = icon;
			Action = action;
		}

		public MultiStringGossipMenuItem(GossipMenuIcon icon, string[] texts, GossipActionHandler callback)
			: this(texts)
		{
			Icon = icon;
			Action = new NonNavigatingGossipAction(callback);
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
			Action = new NonNavigatingGossipAction(callback);
		}

		public GossipMenuItem(string text, GossipActionHandler callback, string confirmText)
			: this(text)
		{
			ConfirmText = confirmText;
			Action = new NonNavigatingGossipAction(callback);
		}

		public GossipMenuItem(string text, GossipActionHandler callback, params GossipMenuItem[] items)
			: this(text)
		{
			Action = new NonNavigatingGossipAction(callback);
			SubMenu = new GossipMenu(items);
		}

		public GossipMenuItem(string text, GossipMenu subMenu)
			: this(text, (IGossipAction)null, subMenu)
		{
		}

		public GossipMenuItem(string text, GossipActionHandler callback, GossipMenu subMenu)
			: this(text)
		{
			Action = new NonNavigatingGossipAction(callback);
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
			Action = new NonNavigatingGossipAction(callback);
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
	public class QuitGossipMenuItem : LocalizedGossipMenuItem
	{
		public QuitGossipMenuItem(GossipMenuIcon type = GossipMenuIcon.Talk, RealmLangKey msg = RealmLangKey.Done)
			: base(type, msg, new object[0])
		{
		}

		public QuitGossipMenuItem(GossipMenuIcon type, RealmLangKey msg, params object[] args)
			: base(type, msg, args)
		{
		}

		public QuitGossipMenuItem(RealmLangKey msg)
			: base(msg, new object[0])
		{
			Action = new NonNavigatingGossipAction((convo) =>
			{
				convo.Character.GossipConversation.StayOpen = false;
			});
		}

		public QuitGossipMenuItem(RealmLangKey msg, params object[] args)
			: base(msg, args)
		{
			Action = new NonNavigatingGossipAction((convo) =>
			{
				convo.Character.GossipConversation.StayOpen = false;
			});
		}

		public QuitGossipMenuItem(GossipActionHandler callback, RealmLangKey msg, params object[] args) :
			base(msg, args)
		{
			Action = new NonNavigatingGossipAction((convo) => {
				convo.Character.GossipConversation.StayOpen = false;
				callback(convo);
			});
		}

		public QuitGossipMenuItem(RealmLangKey text, GossipActionHandler callback, params GossipMenuItem[] items)
			: base(text, items)
		{
			Action = new NonNavigatingGossipAction((convo) => {
				convo.Character.GossipConversation.StayOpen = false;
				callback(convo);
			});
		}

		public QuitGossipMenuItem(GossipMenu subMenu, RealmLangKey msg, params object[] args)
			: base(subMenu, msg, args)
		{
			Action = new NonNavigatingGossipAction((convo) => {
				convo.Character.GossipConversation.StayOpen = false;
			});
		}

		public QuitGossipMenuItem(GossipActionHandler callback, GossipMenu subMenu, RealmLangKey msg, params object[] args)
			: base(subMenu, msg, args)
		{
			Action = new NonNavigatingGossipAction((convo) => {
				convo.Character.GossipConversation.StayOpen = false;
				callback(convo);
			});
		}

		public QuitGossipMenuItem(RealmLangKey text, params GossipMenuItem[] items)
			: base(text, items)
		{
			Action = new NonNavigatingGossipAction((convo) => {
				convo.Character.GossipConversation.StayOpen = false;
			});
		}

		public QuitGossipMenuItem(GossipMenuIcon icon, RealmLangKey text, params GossipMenuItem[] items)
			: base(icon, text, items)
		{
			Action = new NonNavigatingGossipAction((convo) => {
				convo.Character.GossipConversation.StayOpen = false;
			});
		}

		public QuitGossipMenuItem(GossipMenuIcon icon, GossipActionHandler callback, RealmLangKey msg, params object[] args)
			: base(icon, msg, args)
		{
			Action = new NonNavigatingGossipAction((convo) => {
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

		public void SetAction(NonNavigatingGossipAction action)
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