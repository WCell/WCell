using System;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.Util.Variables;

namespace WCell.RealmServer.GameObjects
{
	/// <summary>
	/// The Selection Manager keeps track of all GOs that have been selected by Staff members
	/// </summary>
	public class GOSelectMgr
	{
		[NotVariable]
		/// <summary>
		/// The SpellId of the DO to be used for marking the selected GO.
		/// Use SpellId.None to disable marking.
		/// </summary>
		public static SpellId MarkerId = SpellId.ABOUTTOSPAWN;

		[NotVariable]
		/// <summary>
		/// The radius of the Marker
		/// </summary>
		public static float MarkerRadius = 8f;

		[NotVariable]
		/// <summary>
		/// The radius in which to look for selectable GOs
		/// </summary>
		public static float MaxSearchRadius = 20f;

		[NotVariable]
		public static float MinSearchAngle = (float)Math.PI;


		public static readonly GOSelectMgr Instance = new GOSelectMgr();

		GOSelectMgr()
		{
		}

		/// <summary>
		/// Tries to select the nearest GO that is in front of the character
		/// </summary>
		/// <returns>The newly selected GO.</returns>
		public GameObject SelectClosest(Character chr)
		{
			var gos = chr.GetObjectsInRadius(MaxSearchRadius, ObjectTypes.GameObject, true, 0);

			var distSq = float.MaxValue;
			GameObject sel = null;
			foreach (GameObject go in gos)
			{
				// TODO: Go by angle instead of distance
				//var angle = chr.GetAngleTowards(go);
				var thisDistSq = chr.GetDistanceSq(go);
				if (sel == null ||
					(go.IsInFrontOf(chr) && thisDistSq < distSq))
				{
					sel = go;
					distSq = thisDistSq;
				}
			}

			this[chr] = sel;

			return sel;
		}

		/// <summary>
		/// Sets the Character's selected GameObject
		/// </summary>
		internal GameObject this[Character chr]
		{
			get
			{
				return chr.ExtraInfo.SelectedGO;
			}
			set
			{
				var info = chr.ExtraInfo;
				Deselect(info);

				if (value != null)
				{
					var selection = new GOSelection(value);
					if (MarkerId != SpellId.None)
					{
						var marker = new DynamicObject(chr, MarkerId, MarkerRadius, value.Region, value.Position);
						selection.Marker = marker;
					}
					info.m_goSelection = selection;
				}
			}
		}

		/// <summary>
		/// Deselects the given Character's current GO
		/// </summary>
		internal void Deselect(ExtraInfo info)
		{
			GOSelection selection = info.m_goSelection;
			if (selection != null)
			{
				selection.Dispose();
				info.m_goSelection = null;
			}
		}
	}

	public class GOSelection : IDisposable
	{

		private GameObject m_GO;

		private DynamicObject m_Marker;

		public GOSelection(GameObject go)
		{
			GO = go;
		}

		public GameObject GO
		{
			get
			{
				if (m_GO != null && !m_GO.IsInWorld)
				{
					return null;
				}
				return m_GO;
			}
			set { m_GO = value; }
		}

		public DynamicObject Marker
		{
			get
			{
				if (m_Marker != null && !m_Marker.IsInWorld)
				{
					return null;
				}
				return m_Marker;
			}
			set { m_Marker = value; }
		}

		#region IDisposable Members

		public void Dispose()
		{
			GO = null;
			if (Marker != null)
			{
				Marker.Delete();
				Marker = null;
			}
		}

		#endregion
	}
}