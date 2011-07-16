using System;
using WCell.Util.Logging;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.RealmServer.Global;
using WCell.RealmServer.Lang;
using WCell.Util;
using WCell.Util.Lang;
using WCell.Util.Variables;
using WCell.RealmServer.NPCs;
using WCell.Constants.NPCs;
using WCell.Constants;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Taxi;
using WCell.RealmServer.NPCs.Pets;
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

		internal static IDictionary<uint, IGossipEntry> GossipEntries = new Dictionary<uint, IGossipEntry>(5000);

		public static IGossipEntry GetEntry(uint id)
		{
			IGossipEntry entry;
			GossipEntries.TryGetValue(id, out entry);
			return entry;
		}

		public static readonly uint DefaultTextId = 91800;		// random number
		public static readonly StaticGossipEntry DefaultGossipEntry = new StaticGossipEntry(DefaultTextId, "Hello there!");

		public static readonly uint DynamicTextId = 91801;		// use this for dynamic text entries

		public static string DefaultGossipGreetingMale
		{
			get { return DefaultGossipEntry.GetText(0).TextMale; }
			set { DefaultGossipEntry.GetText(0).TextMale = value; }
		}

		public static string DefaultGossipGreetingFemale
		{
			get { return DefaultGossipEntry.GetText(0).TextFemale; }
			set { DefaultGossipEntry.GetText(0).TextFemale = value; }
		}

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

		public static void LoadAll()
		{
			LoadEntries();
			LoadNPCRelations();
		}

		[Initialization(InitializationPass.Third)]
		public static void LoadEntries()
		{
			ContentMgr.Load<StaticGossipEntry>();
		}

		/// <summary>
		/// Automatically called after NPCs are initialized
		/// </summary>
		internal static void LoadNPCRelations()
		{
			if (ContentMgr.Load<NPCGossipRelation>())
			{
				AddDefaultGossipOptions();
			}
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
					if (entry.NPCFlags.HasAnyFlag(NPCFlags.Gossip))
					{
						var menu = entry.DefaultGossip;
						if (menu == null)
						{
							entry.DefaultGossip = menu = new GossipMenu();
						}
						//else if (menu.GossipItems.Count > 0)
						//{
						//    // Talk option -> Should it be in a sub menu?
						//    entry.DefaultGossip = menu = new GossipMenu(menu.GossipEntry, new GossipMenuItem("Talk")
						//    {
						//        SubMenu = menu
						//    });
						//}

						// NPC professions
						if (entry.NPCFlags.HasAnyFlag(NPCFlags.Banker))
						{
							menu.AddItem(new LocalizedGossipMenuItem(GossipMenuIcon.Bank, convo =>
							{
								convo.Character.OpenBank(convo.Speaker);
							}, RealmLangKey.GossipOptionBanker));
						}
						if (entry.NPCFlags.HasAnyFlag(NPCFlags.BattleMaster))
						{
							menu.AddItem(new GossipMenuItem(GossipMenuIcon.Battlefield, "Battlefield...", convo =>
							{
								if (entry.BattlegroundTemplate != null)
								{
									((NPC)convo.Speaker).TalkToBattlemaster(convo.Character);
								}
							}));
						}
						if (entry.NPCFlags.HasAnyFlag(NPCFlags.InnKeeper))
						{
							menu.AddItem(new LocalizedGossipMenuItem(GossipMenuIcon.Bind, convo =>
							{
								convo.Character.BindTo((NPC)convo.Speaker);
							}, RealmLangKey.GossipOptionInnKeeper));
						}
						if (entry.NPCFlags.HasAnyFlag(NPCFlags.GuildBanker))
						{
							menu.AddItem(new GossipMenuItem(GossipMenuIcon.Guild, "Guild Bank...", convo =>
							{
								convo.Character.SendSystemMessage(RealmLangKey.FeatureNotYetImplemented);
							}));
						}
						if (entry.NPCFlags.HasAnyFlag(NPCFlags.SpiritHealer))
						{
							menu.AddItem(new LocalizedGossipMenuItem(GossipMenuIcon.Resurrect, convo =>
							{
								convo.Character.ResurrectWithConsequences();
							}, RealmLangKey.GossipOptionSpiritHealer));
						}
						if (entry.NPCFlags.HasAnyFlag(NPCFlags.Petitioner))
						{
							menu.AddItem(new GossipMenuItem(GossipMenuIcon.Bank, "Petitions...", convo =>
							{
								((NPC)convo.Speaker).SendPetitionList(convo.Character);
							}));
						}
						if (entry.NPCFlags.HasAnyFlag(NPCFlags.TabardDesigner))
						{
							menu.AddItem(new LocalizedGossipMenuItem(GossipMenuIcon.Tabard, convo =>
							{
								convo.Character.SendSystemMessage(RealmLangKey.FeatureNotYetImplemented);
							}, RealmLangKey.GossipOptionTabardDesigner));
						}
						if (entry.NPCFlags.HasAnyFlag(NPCFlags.FlightMaster))
						{
							menu.AddItem(new LocalizedGossipMenuItem(GossipMenuIcon.Taxi, convo =>
							{
								((NPC)convo.Speaker).TalkToFM(convo.Character);
							}, RealmLangKey.GossipOptionFlightMaster));
						}
						if (entry.NPCFlags.HasAnyFlag(NPCFlags.StableMaster))
						{
							menu.AddItem(new LocalizedGossipMenuItem(convo =>
							{
								PetMgr.ListStabledPets(convo.Character, ((NPC)convo.Speaker));
							}, RealmLangKey.GossipOptionStableMaster));
						}
						if (entry.NPCFlags.HasAnyFlag(NPCFlags.AnyTrainer))
						{
							menu.AddItem(new LocalizedGossipMenuItem(GossipMenuIcon.Train, convo =>
							{
								((NPC)convo.Speaker).TalkToTrainer(convo.Character);
							}, RealmLangKey.GossipOptionTrainer));
						}
						if (entry.NPCFlags.HasAnyFlag(NPCFlags.AnyVendor))
						{
							menu.AddItem(new LocalizedGossipMenuItem(GossipMenuIcon.Trade, convo =>
							{
								if (((NPC)convo.Speaker).VendorEntry != null)
								{
									((NPC)convo.Speaker).VendorEntry.UseVendor(convo.Character);
								}
							}, RealmLangKey.GossipOptionVendor));
						}
					}
				}
			}
		}

		#endregion

		#region Methods
		public static void AddEntry(StaticGossipEntry entry)
		{
			GossipEntries[entry.GossipId] = entry;
		}

		public static void AddText(uint id, params StaticGossipText[] entries)
		{
			GossipEntries[id] =
				new StaticGossipEntry
				{
					GossipId = id,
					GossipTexts = entries
				};
		}
		#endregion
	}
}