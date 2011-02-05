using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Gossips;

namespace WCell.RealmServer.Editor
{
	public class MapEditorMenu : GossipMenu
	{
		public MapEditorMenu(MapEditor editor)
		{
			Editor = editor;
		}

		public MapEditor Editor { get; private set; }

	}
}
