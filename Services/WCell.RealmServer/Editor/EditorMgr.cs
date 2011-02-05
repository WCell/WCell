using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Gossips;
using WCell.Util;
using WCell.Util.Collections;

namespace WCell.RealmServer.Editor
{
	public static class EditorMgr
	{
		public static readonly IDictionary<Map, MapEditor> EditorsByMap = new SynchronizedDictionary<Map, MapEditor>();
		public static readonly IDictionary<Character, MapEditor> EditorsByCharacter = new SynchronizedDictionary<Character, MapEditor>();

		public static MapEditor GetEditor(Map map)
		{
			MapEditor editor;
			EditorsByMap.TryGetValue(map, out editor);
			return editor;
		}

		public static MapEditor GetEditor(Character chr)
		{
			MapEditor editor;
			EditorsByCharacter.TryGetValue(chr, out editor);
			return editor;
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
			var editor = GetEditor(chr);
			if (editor != null)
			{
				editor.Leave(chr);
			}
		}
	}
}
