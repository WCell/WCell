using System.Collections.Generic;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Gossips;

namespace WCell.RealmServer.Editor
{
	/// <summary>
	/// Every map can have one MapEditor.
	/// It contains all the information related to editing that map (mostly spawn editing).
	/// </summary>
	public class MapEditor
	{
		/// <summary>
		/// Set of Characters who are currently working with this editor.
		/// Only use in the Map's context.
		/// </summary>
		public readonly HashSet<Character> Team = new HashSet<Character>();

		public MapEditor(Map map)
		{
			Map = map;
			Menu = new MapEditorMenu(this);
		}

		public GossipMenu Menu
		{
			get;
			private set;
		}

		public Map Map
		{
			get;
			private set;
		}

		/// <summary>
		/// Adds the given Character to the team of this editor (if not already part of the team)
		/// </summary>
		public void Join(Character chr)
		{
			Map.AddMessage(() => { Team.Add(chr); });
		}

		public void Leave(Character chr)
		{
			Map.AddMessage(() => { Team.Remove(chr); });
		}
	}
}
