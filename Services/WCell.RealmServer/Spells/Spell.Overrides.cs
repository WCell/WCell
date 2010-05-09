using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Global;
using WCell.Util.Data;
using WCell.RealmServer.Entities;
using WCell.Util.Graphics;
using World = WCell.RealmServer.Global.World;
using WCell.Constants.World;

namespace WCell.RealmServer.Spells
{
	public enum RequiredSpellTargetType
	{
		Default = -1,
		GameObject = 0,
		NPCAlive,
		NPCDead
	}

	public partial class Spell
	{
		#region Spell Targets
		[Persistent]
		public RequiredSpellTargetType RequiredTargetType = RequiredSpellTargetType.Default;

		public bool MatchesRequiredTargetType(WorldObject obj)
		{
			if (RequiredTargetType == RequiredSpellTargetType.GameObject)
			{
				return obj is GameObject;
			}
			return obj is NPC && ((NPC) obj).IsAlive == (RequiredTargetType == RequiredSpellTargetType.NPCAlive);
		}

		[Persistent]
		public uint RequiredTargetId;

		[Persistent]
		public SpellTargetLocation TargetLocation;

		[Persistent]
		public float TargetOrientation;
		#endregion

		#region Loading
		public void FinalizeDataHolder()
		{
		}
		#endregion
	}

	public class SpellTargetLocation : IWorldLocation
	{
		private Vector3 m_Position;

		public SpellTargetLocation()
		{
		}

		public SpellTargetLocation(MapId region, Vector3 pos)
		{
			Position = pos;
			RegionId = region;
		}
		public Vector3 Position
		{
			get { return m_Position; }
			set { m_Position = value; }
		}

		[Persistent]
		public MapId RegionId
		{
			get;
			set;
		}

		[Persistent]
		public float X
		{
			get { return m_Position.X; }
			set { m_Position.X = value; }
		}

		[Persistent]
		public float Y
		{
			get { return m_Position.Y; }
			set { m_Position.Y = value; }
		}

		[Persistent]
		public float Z
		{
			get { return m_Position.Z; }
			set { m_Position.Z = value; }
		}

		public Region Region
		{
			get { return World.GetRegion(RegionId); }
		}
	}
}
