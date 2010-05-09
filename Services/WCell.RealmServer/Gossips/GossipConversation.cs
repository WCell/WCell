using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Quests;

namespace WCell.RealmServer.Gossips
{
	/// <summary>
	/// Represents specific gossip conversation between character and an object
	/// </summary>
	public class GossipConversation
	{
		#region Constructors
		/// <summary>
		/// Creates gossip conversation by its fields
		/// </summary>
		/// <param name="menu">starting menu</param>
		/// <param name="speaker">character which started the conversation</param>
		/// <param name="target">respondent</param>
		public GossipConversation(GossipMenu menu, Character speaker, WorldObject target, bool keepOpen)
		{
			if (menu.BodyTextId == 0)
			{
				menu.BodyTextId = GossipMgr.DefaultTextId;
			}
			CurrentMenu = menu;
			Character = speaker;
			Speaker = target;
			StayOpen = keepOpen;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Current menu
		/// </summary>
		public GossipMenu CurrentMenu { get; protected internal set; }

		public IUser User { get { return Character; } }

		/// <summary>
		/// Character who initiated the conversation
		/// </summary>
		public Character Character { get; protected set; }

		/// <summary>
		/// The speaker that the Character is talking to (usually an NPC)
		/// </summary>
		public WorldObject Speaker { get; protected set; }

		/// <summary>
		/// If set to true, will always keep the menu open until
		/// (preferrable some Option) set this to false or the client cancelled it.
		/// </summary>
		public bool StayOpen
		{
			get;
			set;
		}
		#endregion

		#region Methods
		/// <summary>
		/// Shows current menu
		/// </summary>
		public void DisplayCurrentMenu()
		{
			DisplayMenu(CurrentMenu);
		}

		/// <summary>
		/// Handles selection of item in menu by player
		/// </summary>
		/// <param name="itemID">ID of selected item</param>
		/// <param name="extra">additional parameter supplied by user</param>
		public void HandleSelectedItem(uint itemID, string extra)
		{
			var gossipItems = CurrentMenu.GossipItems;

			if (itemID >= gossipItems.Count)
				return;

			var item = gossipItems[(int)itemID];

			if (item == null)
				return;

			if (item.Action != null && item.Action.CanUse(Character))
			{
				var menu = CurrentMenu;
				item.Action.OnSelect(this);
				if (menu != CurrentMenu || item.Action.Navigates)
				{
					return;
				}
			}

			if (item.SubMenu != null)
			{
				DisplayMenu(item.SubMenu);
			}
			else if (StayOpen)
			{
				DisplayCurrentMenu();
			}
			else
			{
				CurrentMenu.NotifyClose(this);
				Dispose();
			}
		}

		/// <summary>
		/// Shows menu to player
		/// 
		/// TODO: Why is this only sending Quest-information? And: Why is this sending Quest information at all?
		/// TODO: Quest handling should be part of a subclass of GossipMenu or similar
		/// </summary>
		/// <param name="menu">menu to show</param>
		public void DisplayMenu(GossipMenu menu)
		{
			CurrentMenu = menu;

			if (Speaker is IQuestHolder && ((IQuestHolder)Speaker).QuestHolderInfo != null)
			{
				var questMenuItems = ((IQuestHolder)Speaker).QuestHolderInfo.GetQuestMenuItems(Character);

				GossipHandler.SendPageToCharacter(this, menu.BodyTextId, menu.GossipItems, questMenuItems);
			}
			else
			{
				GossipHandler.SendPageToCharacter(this, menu.BodyTextId, menu.GossipItems, null);
			}
		}

		public void Dispose()
		{
			GossipHandler.SendConversationComplete(Character);

			var conversation = Character.GossipConversation;

			if (conversation != this)
			{
				return;
			}

			Character.GossipConversation = null;
			CurrentMenu = null;
			Speaker = null;
		}

		/// <summary>
		/// Cancels the current conversation
		/// </summary>
		public void Cancel()
		{
			GossipHandler.SendConversationComplete(Character);
			Dispose();
		}

		/// <summary>
		/// Closes any open Client menu and sends the CurrentMenu
		/// </summary>
		public void Invalidate()
		{
			GossipHandler.SendConversationComplete(Character);
			DisplayCurrentMenu();
		}
		#endregion

		public void GoBack()
		{
			if (CurrentMenu.ParentMenu != null)
			{
				DisplayMenu(CurrentMenu.ParentMenu);
			}
			else
			{
				DisplayCurrentMenu();
			}
		}
	}
}
