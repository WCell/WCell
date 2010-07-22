using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Global;
using WCell.RealmServer.Misc;
using WCell.Util.Data;
using WCell.RealmServer.Entities;
using WCell.Util.Graphics;
using World = WCell.RealmServer.Global.World;
using WCell.Constants.World;

namespace WCell.RealmServer.Spells
{
	public partial class Spell
	{
		#region Spell Variables (that may be modified by spell customizations)
		/// <summary>
		/// Wheter this spell can be cast on players (automatically false for all taunts)
		/// </summary>
		public bool CanCastOnPlayer = true;

		/// <summary>
		/// Whether this is a Spell that is only used to prevent other Spells (cannot be cancelled etc)
		/// </summary>
		public bool IsPreventionDebuff;

		/// <summary>
		/// Whether this is an Aura that can override other instances of itself if they have the same rank (true by default).
		/// Else the spell cast will fail when trying to do so.
		/// </summary>
		public bool CanOverrideEqualAuraRank = true;

		/// <summary>
		/// Spells casted whenever this Spell is casted
		/// </summary>
		public Spell[] TargetTriggerSpells, CasterTriggerSpells;

		/// <summary>
		/// Set of specific Spells which, when used, can proc this Spell.
		/// </summary>
		public HashSet<Spell> CasterProcSpells;

		/// <summary>
		/// Set of specific Spells which can proc this Spell on their targets.
		/// </summary>
		public HashSet<Spell> TargetProcSpells;

		/// <summary>
		/// ProcHandlers to be added to the caster of this Spell.
		/// If this is != null, the resulting Aura of this Spell will not be added as a Proc handler itself.
		/// </summary>
		public List<ProcHandlerTemplate> CasterProcHandlers;

		/// <summary>
		/// ProcHandlers to be added to the targets of this Spell.
		/// If this is != null, the resulting Aura of this Spell will not be added as a Proc handler itself.
		/// </summary>
		public List<ProcHandlerTemplate> TargetProcHandlers;

		/// <summary>
		/// Used for teleport spells amongst others
		/// </summary>
		public Vector3 SpellTargetLocation;

		/// <summary>
		/// Wheter this Aura can proc
		/// </summary>
		public bool IsProc;

		/// <summary>
		/// Amount of millis before this Spell may proc another time (if it is a proc)
		/// </summary>
		public int ProcDelay;

		/// <summary>
		/// Whether this Spell's spell damage is increased by AP
		/// </summary>
		public bool DamageIncreasedByAP;
		#endregion

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
	
	public enum RequiredSpellTargetType
	{
		Default = -1,
		GameObject = 0,
		NPCAlive,
		NPCDead
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