using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Entities;
using WCell.Util.Collections;

namespace WCell.Addons.Terrain
{
	/// <summary>
	/// Helps with visualizing Terrain-related data
	/// </summary>
	public static class TerrainVisualizations
	{
		public static readonly SynchronizedDictionary<Character, List<NavFigurine>> Figurines =
			new SynchronizedDictionary<Character, List<NavFigurine>>();

		/// <summary>
		/// Returns an empty list of figurines that contain figurines, associated with the given character
		/// </summary>
		public static List<NavFigurine> ClearVis(Character chr)
		{
			List<NavFigurine> list;
			if (Figurines.TryGetValue(chr, out list))
			{
				foreach (var fig in list)
				{
					fig.Delete();
				}
				chr.SendMessage("Cleared all previous visualization.");
			}
			else
			{
				Figurines.Add(chr, list = new List<NavFigurine>());
			}
			return list;
		}

	}
}
