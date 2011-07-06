using System;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.Core.Paths;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Spells;
using WCell.Util.Graphics;

namespace WCell.Addons.Terrain
{
	/// <summary>
	/// Changes some spells to support Terrain debugging efforts
	/// </summary>
	public static class DebugSpells
	{
		public static readonly SpellId PathDisplaySpell = SpellId.UnusedTowerCaptureTestDND;

		[Initialization(InitializationPass.Second)]
		public static void FixDisplayPathSpell()
		{
			var spell = SpellHandler.Get(PathDisplaySpell);
			spell.SpecialCast = DisplayPath;
		}

		private static void DisplayPath(Spell spell, WorldObject caster, WorldObject target, ref Vector3 to)
		{
			if (!(caster is Character)) return;
			if (!NPCMgr.Loaded)
			{
				NPCMgr.LoadAllLater();
				caster.Say("Loading NPCs...");
				return;
			}


			var from = caster.Position;
			from.Z += 5;
			to.Z += 5;
			caster.Map.Terrain.FindPath(new PathQuery(from, ref to, caster, OnFoundPath));
		}

		private static void OnFoundPath(PathQuery query)
		{
			var chr = (Character)query.ContextHandler;
			var figurines = TerrainVisualizations.ClearVis(chr);

			// spawn figurines along the path
			if (query.Path == null)
			{
				chr.SendSystemMessage("Could not find path");
				return;
			}

			var last = new NavFigurine(chr.Map, query.From);
			foreach (var vert in query.Path)
			{
				var fig = new NavFigurine(chr.Map, vert);

				var v = vert;
				last.SetOrientationTowards(ref v);
				last.ChannelObject = fig;

				figurines.Add(fig);
				last = fig;
			}
		}
	}
}
