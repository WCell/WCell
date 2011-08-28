using System;
using System.Collections.Generic;
using NLog;
using WCell.Constants;
using WCell.RealmServer.Lang;

namespace WCell.RealmServer.Gossips
{
	public abstract class DynamicTextGossipMenu : GossipMenu
	{
		private static readonly DynamicGossipEntry SharedEntry = new DynamicGossipEntry(88993333, new DynamicGossipText(OnTextQuery));

		private static string OnTextQuery(GossipConversation convo)
		{
			return ((DynamicTextGossipMenu)convo.CurrentMenu).GetText(convo);
		}

		protected DynamicTextGossipMenu()
			: base(SharedEntry)
		{
		}

		protected DynamicTextGossipMenu(params GossipMenuItemBase[] items)
			: base(SharedEntry, items)
		{
		}

		public abstract string GetText(GossipConversation convo);
	}

	/// <summary>
	/// Represents single menu in conversation with it's items
	/// </summary>
	public class GossipMenu
	{
		public static readonly GossipMenuItem[] EmptyGossipItems = new GossipMenuItem[0];

		private IGossipEntry m_textEntry;
		private List<GossipMenuItemBase> m_gossipItems;
		private GossipMenu m_parent;

		/// <summary>
		/// Default constructor
		/// </summary>
		public GossipMenu()
		{
			m_textEntry = GossipMgr.DefaultGossipEntry;
		}

		/// <summary>
		/// Constructor initializing menu with body text ID
		/// </summary>
		/// <param name="bodyTextID">GossipEntry Id</param>
		public GossipMenu(uint bodyTextID)
		{
			m_textEntry = GossipMgr.GetEntry(bodyTextID);
			if (m_textEntry == null)
			{
				m_textEntry = GossipMgr.DefaultGossipEntry;
				LogManager.GetCurrentClassLogger().Warn("Tried to create GossipMenu with invalid GossipEntry id: " + bodyTextID);
			}
		}

		public GossipMenu(IGossipEntry textEntry)
		{
			m_textEntry = textEntry;
			if (m_textEntry == null)
			{
				throw new ArgumentNullException("textEntry");
			}
		}

		public GossipMenu(uint bodyTextId, List<GossipMenuItemBase> items)
			: this(bodyTextId)
		{
			m_gossipItems = items;
		}

		public GossipMenu(uint bodyTextId, params GossipMenuItem[] items)
			: this(bodyTextId)
		{
			m_gossipItems = new List<GossipMenuItemBase>(items.Length);
			foreach (var item in items)
			{
				CheckItem(item);
				m_gossipItems.Add(item);
			}
		}

		public GossipMenu(uint bodyTextId, params GossipMenuItemBase[] items)
			: this(bodyTextId)
		{
			m_gossipItems = new List<GossipMenuItemBase>(items.Length);
			foreach (var item in items)
			{
				CheckItem(item);
				m_gossipItems.Add(item);
			}
		}

		public GossipMenu(IGossipEntry text, List<GossipMenuItemBase> items)
			: this(text)
		{
			m_gossipItems = items;
		}

		public GossipMenu(IGossipEntry text, params GossipMenuItemBase[] items)
			: this(text)
		{
			m_gossipItems = new List<GossipMenuItemBase>(items.Length);
			foreach (var item in items)
			{
				CheckItem(item);
				m_gossipItems.Add(item);
			}
		}

		public GossipMenu(params GossipMenuItemBase[] items)
			: this()
		{
			m_gossipItems = new List<GossipMenuItemBase>(items.Length);
			foreach (var item in items)
			{
				CheckItem(item);
				m_gossipItems.Add(item);
			}
		}

		public GossipMenu ParentMenu
		{
			get { return m_parent; }
		}

		public IList<GossipMenuItemBase> GossipItems
		{
			get
			{
				if (m_gossipItems == null)
					return EmptyGossipItems;

				return m_gossipItems;
			}
		}

		/// <summary>
		/// ID of text in the body of this menu
		/// </summary>
		public IGossipEntry GossipEntry
		{
			get
			{
				return m_textEntry;
			}
			set { m_textEntry = value; }
		}

		/// <summary>
		/// Will keep resending the Gump until deactivated (usually using a Quit button)
		/// </summary>
		public bool KeepOpen
		{
			get;
			set;
		}

		public void AddRange(params GossipMenuItemBase[] items)
		{
			foreach (var item in items)
			{
				CheckItem(item);
				m_gossipItems.Add(item);
			}
		}

		protected void CheckItem(GossipMenuItemBase item)
		{
			if (item.SubMenu != null)
			{
				var sub = item.SubMenu;
				sub.m_parent = this;
			}
		}

		public void AddItem(GossipMenuItemBase item)
		{
			if (m_gossipItems == null)
			{
				m_gossipItems = new List<GossipMenuItemBase>(1);
			}

			CheckItem(item);
			m_gossipItems.Add(item);
		}

		/// <summary>
		/// Replaces the item at the given index with the given item.
		/// If index == count, appends item to end.
		/// </summary>
		public void SetItem(int index, GossipMenuItemBase item)
		{
			if (m_gossipItems == null)
			{
				m_gossipItems = new List<GossipMenuItemBase>(1);
			}

			CheckItem(item);
			if (index == m_gossipItems.Count)
			{
				// append to end
				m_gossipItems.Add(item);
			}
			else
			{
				// replace
				m_gossipItems[index] = item;
			}
		}

		public void AddItem(GossipMenuIcon type)
		{
			AddItem(new GossipMenuItem(type, type.ToString()));
		}

		public void AddItem(int index, GossipMenuItemBase item)
		{
			if (m_gossipItems == null)
				m_gossipItems = new List<GossipMenuItemBase>(1);

			if (item != null)
			{
				CheckItem(item);
				m_gossipItems.Insert(index, item);
			}
		}

		public void AddQuitMenuItem(RealmLangKey msg = RealmLangKey.Done)
		{
			AddItem(new QuitGossipMenuItem(msg, new object[0]));
		}

		public void AddQuitMenuItem(RealmLangKey msg, params object[] args)
		{
			AddItem(new QuitGossipMenuItem(msg, args));
		}

		public void AddQuitMenuItem(GossipActionHandler callback, RealmLangKey msg = RealmLangKey.Done, params object[] args)
		{
			AddItem(new QuitGossipMenuItem(callback, msg, args));
		}

		public void AddGoBackItem()
		{
			AddGoBackItem("Go back...");
		}

		public void AddGoBackItem(string text)
		{
			var action = new NavigatingGossipAction(convo =>
			{
				convo.Character.GossipConversation.GoBack();
			});
			AddItem(new GossipMenuItem(text, action));
		}

		public void AddGoBackItem(string text, GossipActionHandler callback)
		{
			var action = new NavigatingGossipAction(convo =>
			{
				callback(convo);
				convo.Character.GossipConversation.GoBack();
			});
			AddItem(new GossipMenuItem(text, action));
		}

		public bool RemoveItem(GossipMenuItemBase item)
		{
			return m_gossipItems.Remove(item);
		}

		public void ClearAllItems()
		{
			m_gossipItems.Clear();
		}

		/// <summary>
		/// Called before menu is sent to Character
		/// </summary>
		protected internal virtual void OnDisplay(GossipConversation convo)
		{
		}

		internal void NotifyClose(GossipConversation convo)
		{
		}

		void Dispose()
		{
			m_gossipItems = null;
			if (m_parent != null)
			{
				m_parent.Dispose();
			}
		}
	}
}