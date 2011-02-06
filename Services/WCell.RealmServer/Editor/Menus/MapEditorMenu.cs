using System;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Lang;
using WCell.RealmServer.NPCs;

namespace WCell.RealmServer.Editor.Menus
{
	public class MapEditorMenu : DynamicTextGossipMenu
	{
		public override string GetText(GossipConversation convo)
		{
			var text = RealmLocalizer.Instance.Translate(convo.Character.Locale, RealmLangKey.EditorMapMenuText) + GossipTextHelper.Newline;

			if (!GOMgr.Loaded || !NPCMgr.Loaded)
			{
				if (!convo.Speaker.HasUpdateAction(action => action is PeriodicLoadMapTimer))
				{
					// already loading
					text += RealmLocalizer.Instance.Translate(convo.Character.Locale, RealmLangKey.EditorMapMenuStatusNoData);
				}
				else
				{
					// not loading yet
					text += RealmLocalizer.Instance.Translate(convo.Character.Locale, RealmLangKey.EditorMapMenuStatusDataLoading);
				}
			}
			else if (!Editor.Map.IsSpawned)
			{
				if (Editor.Map.IsSpawning)
				{
					// already spawning
					text += RealmLocalizer.Instance.Translate(convo.Character.Locale, RealmLangKey.EditorMapMenuStatusSpawning);
				}
				else
				{
					// not spawning yet
					text += RealmLocalizer.Instance.Translate(convo.Character.Locale, RealmLangKey.EditorMapMenuStatusNotSpawned);
				}
			}
			return text;
		}

		public MapEditorMenu(MapEditor editor)
		{
			Editor = editor;
			KeepOpen = true;

			AddItem(new LocalizedGossipMenuItem(OnLoadClicked,
				convo => (!GOMgr.Loaded || !NPCMgr.Loaded) &&		// timer not running yet
					!convo.Speaker.HasUpdateAction(action => action is PeriodicLoadMapTimer),
				RealmLangKey.EditorMapMenuLoadData));

			AddItem(new LocalizedGossipMenuItem(convo =>
				{
					Editor.Map.SpawnMapLater();
					convo.Character.AddMessage(convo.Invalidate);		// show menu again, when done spawning
				},
				convo => GOMgr.Loaded && NPCMgr.Loaded && !Editor.Map.IsSpawned && !Editor.Map.IsSpawning,
				RealmLangKey.EditorMapMenuSpawnMap));

			AddItem(new LocalizedGossipMenuItem(convo =>
				{
					Editor.Map.ClearLater();
					convo.Character.AddMessage(convo.Invalidate);		// show menu again, when done clearing
				},
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


			// leave the editor
			AddQuitMenuItem(convo => Editor.Leave(convo.Character));
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

		class PeriodicLoadMapTimer : ObjectUpdateTimer
		{
			private readonly GossipConversation m_Convo;

			public PeriodicLoadMapTimer(GossipConversation convo)
			{
				m_Convo = convo;
				Delay = 1000;
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
