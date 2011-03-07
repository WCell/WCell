using System;
using WCell.RealmServer.Editor.Figurines;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Lang;
using WCell.RealmServer.NPCs.Spawns;

namespace WCell.RealmServer.Editor.Menus
{
	public class SpawnPointEditorMenu : SpawnEditorMenu
	{
		public override string GetText(GossipConversation convo)
		{
			return SpawnPoint.ToString();
		}

		public SpawnPointEditorMenu(MapEditor editor, NPCSpawnPoint spawnPoint, EditorFigurine figurine)
			: base(editor, spawnPoint, figurine)
		{
			AddItem(new LocalizedGossipMenuItem(convo => MoveTo(convo.Character),
												//convo => ,
												RealmLangKey.EditorSpawnPointMenuMoveOverHere));

			AddQuitMenuItem();
		}

		private void MoveTo(Character chr)
		{
			SpawnPoint.SpawnEntry.Position = chr.Position;
			Figurine.TeleportTo(chr.Position);
		}
	}
}
