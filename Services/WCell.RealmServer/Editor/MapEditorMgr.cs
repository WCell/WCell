using System.Collections.Generic;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util.Collections;

namespace WCell.RealmServer.Editor
{
	public static class MapEditorMgr
	{
		/// <summary>
		/// All active Editors
		/// </summary>
		public static readonly IDictionary<Map, MapEditor> EditorsByMap = new SynchronizedDictionary<Map, MapEditor>();

		/// <summary>
		/// Everyone who is or was recently using the editor
		/// </summary>
		public static readonly IDictionary<Character, EditorArchitectInfo> Architects = new SynchronizedDictionary<Character, EditorArchitectInfo>();

		public static MapEditor GetEditor(Map map)
		{
			MapEditor editor;
			EditorsByMap.TryGetValue(map, out editor);
			return editor;
		}

		public static EditorArchitectInfo GetArchitectInfo(Character chr)
		{
			EditorArchitectInfo info;
			Architects.TryGetValue(chr, out info);
			return info;
		}

		public static MapEditor GetOrCreateEditor(Map map)
		{
			MapEditor editor;
			if (!EditorsByMap.TryGetValue(map, out editor))
			{
				EditorsByMap.Add(map, editor = new MapEditor(map));
			}
			return editor;
		}

		public static EditorArchitectInfo GetOrCreateArchitectInfo(Character chr)
		{
			EditorArchitectInfo info;
			if (!Architects.TryGetValue(chr, out info))
			{
				Architects.Add(chr, info = new EditorArchitectInfo(chr));
			}
			return info;
		}

		public static MapEditor StartEditing(Map map, Character chr = null)
		{
			var editor = GetOrCreateEditor(map);
			if (chr != null)
			{
				editor.Join(chr);
			}
			return editor;
		}

		public static void StopEditing(Character chr)
		{
			var architect = GetArchitectInfo(chr);
			if (architect != null)
			{
				architect.Editor.Leave(chr);
			}
		}
	}
}
