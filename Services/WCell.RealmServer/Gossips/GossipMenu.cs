using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Lang;

namespace WCell.RealmServer.Gossips
{
	/// <summary>
	/// Represents single menu in conversation with it's items
	/// </summary>
	public class GossipMenu
	{
		public static readonly GossipMenuItem[] EmptyGossipItems = new GossipMenuItem[0];

		private uint m_bodyTextID;
		private List<GossipMenuItemBase> m_gossipItems;
		private GossipMenu m_parent;

		/// <summary>
		/// Default constructor
		/// </summary>
		public GossipMenu()
		{
		}

		/// <summary>
		/// Constructor initializing menu with body text ID
		/// </summary>
		/// <param name="bodyTextID"><see cref="BodyTextId"/></param>
		public GossipMenu(uint bodyTextID)
		{
			m_bodyTextID = bodyTextID;
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
		public uint BodyTextId
		{
			get
			{
				return m_bodyTextID;
			}
			set { m_bodyTextID = value; }
		}

		/// <summary>
		/// Will keep resending the Gump until deactivated (usually using a Quit button)
		/// </summary>
		public bool KeepOpen
		{
			get;
			set;
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

		public GossipMenu(params GossipMenuItemBase[] items)
		{
			m_gossipItems = new List<GossipMenuItemBase>(items.Length);
			foreach (var item in items)
			{
				CheckItem(item);
				m_gossipItems.Add(item);
			}
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

		public int GetValidItemsCount(Character speaker)
		{
			var gossipItems = GossipItems;
			var count = gossipItems.Count;

			for (int i = 0; i < gossipItems.Count; i++)
			{
				bool shouldShow = true;

				if (gossipItems[i].Action != null && !gossipItems[i].Action.CanUse(speaker))
					shouldShow = false;

				if (!shouldShow)
				{
					gossipItems[i] = null;
					count--;
				}
			}
			return count;
		}

		public void AddItem(GossipMenuItemBase item)
		{
			if (m_gossipItems == null)
				m_gossipItems = new List<GossipMenuItemBase>(1);

			if (item != null)
			{
				CheckItem(item);
				m_gossipItems.Add(item);
			}
		}

		public void AddItem(GossipMenuIcon type)
		{
			if (m_gossipItems == null)
				m_gossipItems = new List<GossipMenuItemBase>(1);

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

		public GossipMenu CreateSubMenu()
		{
			return new GossipMenu(m_bodyTextID + 1000);
		}

		public void AddQuitMenuItem(RealmLangKey msg = RealmLangKey.Done)
		{
			AddItem(new QuitGossipMenuItem(msg, new object[0]));
		}

		public void AddQuitMenuItem(RealmLangKey msg, params object[] args)
		{
			AddItem(new QuitGossipMenuItem(msg, args));
		}

		public void AddQuitMenuItem(string text, GossipActionHandler callback)
		{
			var action = new DefaultGossipAction(convo => {
				callback(convo);
				convo.Character.GossipConversation.StayOpen = false;
			});
			AddItem(new GossipMenuItem(text, action));
		}

		public void AddGoBackItem()
		{
			AddGoBackItem("Go back...");
		}

		public void AddGoBackItem(string text)
		{
			var action = new NavigationGossipAction(convo => {
				convo.Character.GossipConversation.GoBack();
			});
			AddItem(new GossipMenuItem(text, action));
		}

		public void AddGoBackItem(string text, GossipActionHandler callback)
		{
			var action = new NavigationGossipAction(convo => {
				callback(convo);
				convo.Character.GossipConversation.GoBack();
			});
			AddItem(new GossipMenuItem(text, action));
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