using System;
using WCell.Constants.World;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util.Graphics;

namespace WCell.RealmServer.GameObjects
{
	public class Portal : GameObject
	{
		public static Portal Create(IWorldLocation target)
		{
			var entry = GOMgr.GetEntry(GOPortalEntry.PortalId);
			if (entry == null)
			{
				return null;
			}
			var portal = (Portal)Create(entry, entry.FirstTemplate);
			portal.Target = target;
			return portal;
		}

		public static Portal Create(MapId rgnId, Vector3 pos, MapId targetRgn, Vector3 targetPos)
		{
			var portal = Create(targetRgn, targetPos);
			var rgn = World.GetRegion(rgnId);
			if (rgn == null)
			{
				throw new ArgumentException("Invalid rgnId (not a Continent): " + rgnId);
			}
			portal.Position = pos;
			rgn.AddObject(portal);
			return portal;
		}

		public static Portal Create(MapId targetRgn, Vector3 targetPos)
		{
			var entry = GOMgr.GetEntry(GOPortalEntry.PortalId);
			if (entry == null)
			{
				return null;
			}
			var portal = (Portal)Create(entry, entry.FirstTemplate);
			portal.Target = new WorldLocation(targetRgn, targetPos);
			return portal;
		}

		public static Portal Create(Region targetRgn, Vector3 targetPos)
		{
			var entry = GOMgr.GetEntry(GOPortalEntry.PortalId);
			if (entry == null)
			{
				return null;
			}
			var portal = (Portal)Create(entry, entry.FirstTemplate);
			portal.Target = new WorldLocation(targetRgn, targetPos);
			return portal;
		}

		IWorldLocation m_Target;

		//public bool IsWalkInPortal = false;

		public Portal()
		{
		}

		protected Portal(IWorldLocation target)
		{
			Target = target;
		}

		/// <summary>
		/// Can be used to set the <see cref="Target"/>
		/// </summary>
		public ZoneId TargetZoneId
		{
			get
			{
				if (Target is IWorldZoneLocation)
				{
					if (((IWorldZoneLocation)Target).ZoneTemplate != null)
					{
						return ((IWorldZoneLocation)Target).ZoneTemplate.Id;
					}
				}
				return ZoneId.None;
			}
			set
			{
				Target = World.GetSite(value);
			}
		}

		/// <summary>
		/// Can be used to set the <see cref="Target"/>
		/// </summary>
		public ZoneTemplate TargetZone
		{
			get
			{
				if (Target is IWorldZoneLocation)
				{
					return ((IWorldZoneLocation)Target).ZoneTemplate;
				}
				return null;
			}
			set
			{
				Target = value.Site;
			}
		}

		/// <summary>
		/// The target to which everyone should be teleported.
		/// </summary>
		public IWorldLocation Target
		{
			get { return m_Target; }
			set
			{
				m_Target = value;
				if (m_Target == null)
				{
					throw new Exception("Target for GOPortalEntry must not be null.");
				}
			}
		}
	}
}