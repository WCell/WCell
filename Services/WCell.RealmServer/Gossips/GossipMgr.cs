using System;
using NLog;
using WCell.Core;
using WCell.RealmServer.Content;
using WCell.RealmServer.Global;
using WCell.Util;
using WCell.Util.Variables;
using WCell.RealmServer.NPCs;
using WCell.Constants.NPCs;
using WCell.Constants;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Taxi;
using System.Collections;
using System.Collections.Generic;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Battlegrounds;

namespace WCell.RealmServer.Gossips
{
	/// <summary>
	/// TODO: Localizations
	/// </summary>
	public sealed class GossipMgr : Manager<GossipMgr>, IDisposable
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		#region Fields
		//private SynchronizedDictionary<WorldObject, GossipMenu> m_gossipMenus;
		//private SynchronizedDictionary<Character, GossipConversation> m_gossipConversations;

		internal static IDictionary<uint,IGossipEntry> NPCTexts = new Dictionary<uint,IGossipEntry>(5000);

		internal static int gossipCount;

		[Variable(false)]
		public static uint DefaultTextId = 91800;
		public static string DefaultTitleMale = "Hello there!";
		public static string DefaultTitleFemale = "Hello there!";

		#endregion

		static GossipMgr()
		{

		}

		#region Properties
		//public SynchronizedDictionary<WorldObject, GossipMenu> GossipMenus
		//{
		//    get
		//    {
		//        return m_gossipMenus;
		//    }
		//}

		//public SynchronizedDictionary<Character, GossipConversation> GossipConversations
		//{
		//    get
		//    {
		//        return m_gossipConversations;
		//    }
		//}
		#endregion

		#region Start/Stop System

		protected override bool InternalStart()
		{
			return true;
		}

		protected override bool InternalStop()
		{
			Dispose();

			return true;
		}

		protected override bool InternalRestart(bool forced)
		{
			if (State == ManagerStates.Error)
			{
				InternalStop();
			}

			return InternalStart();
		}

		#endregion

		#region IDisposable

		public void Dispose()
		{

			foreach (var chr in World.GetAllCharacters())
			{
				if (chr.GossipConversation != null)
				{
					chr.GossipConversation = null;
				}
			}
		}

		#endregion

		#region Initializing and Loading

		public static bool Loaded
		{
			get;
			private set;
		}

		static void LoadAll()
		{
			Loaded = true;

			ContentHandler.Load<GossipEntry>();
			AddDefaultGossipOptions();
		}

		/// <summary>
		/// Add default Gossip options for Vendors etc
		/// </summary>
		private static void AddDefaultGossipOptions()
		{
			foreach (var _entry in NPCMgr.Entries)
			{
				var entry = _entry;					// copy object to prevent access to modified closure in callbacks
				if (entry != null)
				{
					if (entry.NPCFlags.HasFlag(NPCFlags.Gossip))
					{
						var menu = entry.DefaultGossip;
						if (menu == null)
						{
							entry.DefaultGossip = menu = new GossipMenu();
						}
						else
						{
							if (menu.GossipItems.Count > 0)
							{
								// Talk option
								entry.DefaultGossip = menu = new GossipMenu(
									menu.BodyTextId,
									new GossipMenuItem
									{
										SubMenu = menu
									});
							}
						}

						// NPC professions
						if (entry.NPCFlags.HasFlag(NPCFlags.Banker))
						{
							menu.AddItem(new GossipMenuItem(GossipMenuIcon.Bank, "Show Bank Account...", convo =>
							{
								convo.Character.OpenBank(convo.Speaker);
							}));
						}
                        if (entry.NPCFlags.HasFlag(NPCFlags.BattleMaster))
						{
							menu.AddItem(new GossipMenuItem(GossipMenuIcon.Battlefield, "Battlefield...", convo =>
							{
								if (entry.BattlegroundTemplate != null)
								{
									((NPC)convo.Speaker).TalkToBattlemaster(convo.Character);
								}
							}));
						}
                        if (entry.NPCFlags.HasFlag(NPCFlags.InnKeeper))
						{
							menu.AddItem(new GossipMenuItem(GossipMenuIcon.Bind, "Bind...", convo =>
							{
								convo.Character.BindTo((NPC)convo.Speaker);
							}));
						}
						if (entry.NPCFlags.HasFlag(NPCFlags.GuildBanker))
						{
							menu.AddItem(new GossipMenuItem(GossipMenuIcon.Guild, "Guild Bank...", convo =>
							{
								convo.Character.SendSystemMessage("Feature Not Yet Implemented");
							}));
						}
                        if (entry.NPCFlags.HasFlag(NPCFlags.SpiritHealer))
						{
							menu.AddItem(new GossipMenuItem(GossipMenuIcon.Resurrect, "Resurrect...", convo =>
							{
								convo.Character.ResurrectSH();
							}));
						}
                        if (entry.NPCFlags.HasFlag(NPCFlags.Petitioner))
						{
							menu.AddItem(new GossipMenuItem(GossipMenuIcon.Bank, "Petitions...", convo =>
							{
								((NPC)convo.Speaker).SendPetitionList(convo.Character);
							}));
						}
                        if (entry.NPCFlags.HasFlag(NPCFlags.TabardDesigner))
						{
							menu.AddItem(new GossipMenuItem(GossipMenuIcon.Tabard, "Tabard...", convo =>
							{
								convo.Character.SendSystemMessage("Feature Not Yet Implemented");
							}));
						}
                        if (entry.NPCFlags.HasFlag(NPCFlags.FlightMaster))
						{
							menu.AddItem(new GossipMenuItem(GossipMenuIcon.Taxi, "Fly...", convo =>
							{
								((NPC)convo.Speaker).TalkToFM(convo.Character);
							}));
						}
                        if (entry.NPCFlags.HasFlag(NPCFlags.AnyTrainer))
						{
							menu.AddItem(new GossipMenuItem(GossipMenuIcon.Train, "Train...", convo =>
							{
								((NPC)convo.Speaker).TalkToTrainer(convo.Character);
							}));
						}
                        if (entry.NPCFlags.HasFlag(NPCFlags.AnyVendor))
						{
							menu.AddItem(new GossipMenuItem(GossipMenuIcon.Trade, "Trade...", convo =>
							{
								if (((NPC)convo.Speaker).VendorEntry != null)
								{
									((NPC)convo.Speaker).VendorEntry.UseVendor(convo.Character);
								}
							}));
						}
					}
				}
			}
		}

		/// <summary>
		/// Automatically called after NPCs are initialized
		/// </summary>
		internal static void EnsureInitialized()
		{
			if (!Loaded)
			{
				LoadAll();
			}
		}

		#endregion

		#region Methods
		public static void AddEntry(GossipEntry entry)
		{
			NPCTexts[entry.GossipId] = entry;
		}

		public static void AddText(uint id, params GossipText[] entries)
		{
			NPCTexts[id] =
				new GossipEntry
				{
					GossipId = id,
					GossipEntries = entries
				};
		}
		#endregion
	}
}