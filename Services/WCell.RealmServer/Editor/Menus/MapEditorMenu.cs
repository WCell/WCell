using System;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Lang;
using WCell.RealmServer.NPCs;

namespace WCell.RealmServer.Editor.Menus
{
	public class MapEditorMenu : GossipMenu
	{
		public MapEditorMenu(MapEditor editor)
		{
			Editor = editor;
			KeepOpen = true;

			AddItem(new LocalizedGossipMenuItem(OnLoadClicked,
												convo => (!GOMgr.Loaded || !NPCMgr.Loaded) &&		// timer not running yet
														!convo.Speaker.HasUpdateAction(action => action is PeriodicLoadMapTimer),
												RealmLangKey.EditorMapMenuLoadData));

			AddItem(new LocalizedGossipMenuItem(convo => Editor.Map.SpawnMap(),
												convo => GOMgr.Loaded && NPCMgr.Loaded && !Editor.Map.IsSpawned,
												RealmLangKey.EditorMapMenuSpawnMap));

			AddItem(new LocalizedGossipMenuItem(convo => Editor.Map.ClearLater(),
												convo => Editor.Map.IsSpawned,
												RealmLangKey.AreYouSure, 
												RealmLangKey.EditorMapMenuClearMap));

			AddItem(new LocalizedGossipMenuItem(convo => Editor.IsVisible = true,
												convo => Editor.Map.IsSpawned && !Editor.IsVisible,
												RealmLangKey.EditorMapMenuShow));

			AddItem(new LocalizedGossipMenuItem(convo => Editor.IsVisible = false,
												convo => Editor.Map.IsSpawned && Editor.IsVisible,
												RealmLangKey.EditorMapMenuHide));

			AddItem(new LocalizedGossipMenuItem(convo => Editor.Map.SpawnPointsEnabled = true,
												convo => !Editor.Map.SpawnPointsEnabled,
												RealmLangKey.EditorMapMenuEnableAllSpawnPoints));

			AddItem(new LocalizedGossipMenuItem(convo => Editor.Map.SpawnPointsEnabled = false,
												convo => Editor.Map.SpawnPointsEnabled,
												RealmLangKey.AreYouSure,
												RealmLangKey.EditorMapMenuDisableAllSpawnPoints));

			AddQuitMenuItem();
		}

		public MapEditor Editor { get; private set; }

		static void OnLoadClicked(GossipConversation convo)
		{
			GOMgr.LoadAllLater();
			NPCMgr.LoadAllLater();

			var chr = convo.Character;
			chr.SendSystemMessage(RealmLangKey.PleaseWait);

			convo.Character.AddUpdateAction(new PeriodicLoadMapTimer(convo));
		}

		class PeriodicLoadMapTimer : SimpleObjectUpdateAction
		{
			private readonly GossipConversation m_Convo;

			public PeriodicLoadMapTimer(GossipConversation convo)
			{
				m_Convo = convo;
				Ticks = 10;
				Callback = OnTick;
			}

			void OnTick(WorldObject obj)
			{
				// invalidate menu, once everything is loaded
				var chr = (Character)obj;
				if (NPCMgr.Loaded && GOMgr.Loaded)
				{
					if (chr.GossipConversation == m_Convo)
					{
						m_Convo.Invalidate();
					}
					chr.RemoveUpdateAction(this);
				}
			}
		}
	}
}
