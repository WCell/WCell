using System;
using System.Collections.Generic;
using WCell.RealmServer.Editor.Figurines;
using WCell.RealmServer.Editor.Menus;
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
		public const int CharacterUpdateDelayMillis = 1000;

		/// <summary>
		/// Set of Characters who are currently working with this editor.
		/// Only use in the Map's context.
		/// </summary>
		public readonly Dictionary<uint, EditorArchitectInfo> Team = new Dictionary<uint, EditorArchitectInfo>();
		private readonly List<EditorFigurine> Figurines = new List<EditorFigurine>(100);

		private bool m_IsVisible;

		public MapEditor(Map map)
		{
			Map = map;
			Menu = new MapEditorMenu(this);
			m_IsVisible = false;
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

		public bool IsVisible
		{
			get { return m_IsVisible; }
			set
			{
				if (m_IsVisible == value) return;

				Map.EnsureContext();
				m_IsVisible = value;
				if (value)
				{
					PlaceFigurines();
				}
				else
				{
					RemoveFigurines();
				}
			}
		}

		private void PlaceFigurines()
		{
			Map.ForeachSpawnPool(pool =>
			{
				foreach (var point in pool.SpawnPoints)
				{
					var figurine = new SpawnPointFigurine(this, point);
					
					Figurines.Add(figurine);
					figurine.TeleportTo(point);
				}
			});
		}

		private void RemoveFigurines()
		{
			//InvalidateVisibilityForTeamMembers();
			foreach (var fig in Figurines)
			{
				fig.Delete();
			}
			Figurines.Clear();
		}

		void InvalidateVisibilityForTeamMembers()
		{
			foreach (var info in Team.Values)
			{
				if (info.Character.IsInContext)
				{
					info.Character.ResetOwnWorld();
				}
			}
		}

		/// <summary>
		/// Adds the given Character to the team of this editor (if not already part of the team)
		/// </summary>
		public void Join(Character chr)
		{
			Map.AddMessage(() =>
			{
				if (chr.Map != Map) return;
				OnJoin(chr);
			});
		}

		public void Leave(Character chr)
		{
			Map.AddMessage(() =>
			{
				OnLeave(chr);
			});
		}

		private void OnJoin(Character chr)
		{
			if (Team.ContainsKey(chr.EntityId.Low)) return;

			var architect = MapEditorMgr.GetOrCreateArchitectInfo(chr);
			Team.Add(chr.EntityId.Low, architect);
			architect.Editor = this;
			chr.CallPeriodically(CharacterUpdateDelayMillis, UpdateCallback);
		}

		void OnLeave(Character chr)
		{
			Team.Remove(chr.EntityId.Low);
			chr.RemoveUpdateAction(UpdateCallback);

			// hide all figurines
			chr.ResetOwnWorld();
		}

		/// <summary>
		/// Called periodically on editing Characters
		/// </summary>
		void UpdateCallback(WorldObject obj)
		{
			// make sure, Character is still in place
			var chr = (Character)obj;
			if (obj.Map != Map || !obj.IsInWorld)
			{
				Leave(chr);
				return;
			}

			// update target
			var info = MapEditorMgr.GetOrCreateArchitectInfo(chr);
			var chrTarget = chr.Target as EditorFigurine;
			var target = info.CurrentTarget;
			if (target != chrTarget)
			{
				target = chrTarget;
				info.CurrentTarget = target;
				if (target != null)
				{
					// selected new target
				}
			}
		}
	}
}
