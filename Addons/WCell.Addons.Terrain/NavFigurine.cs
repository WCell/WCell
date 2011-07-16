using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.RealmServer.Editor.Figurines;
using WCell.RealmServer.Global;
using WCell.Util.Graphics;

namespace WCell.Addons.Terrain
{
	/// <summary>
	/// Figurine used to visualize certain things in the terrain
	/// </summary>
	public class NavFigurine : FigurineBase
	{
		public const NPCId NavFigurineId = NPCId.DrunkDwarfReveler;

		public NavFigurine()
			: base(NavFigurineId)		// NPCId doesnt matter
		{
			ChannelSpell = SpellId.ClassSkillDrainSoulRank5;
		}

		public NavFigurine(Map map, Vector3 location) : 
			this()
		{
			map.AddObject(this, ref location);
		}
	}
}
