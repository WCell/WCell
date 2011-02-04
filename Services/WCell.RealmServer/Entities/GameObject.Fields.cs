/*************************************************************************
 *
 *   file		: GameObject.Fields.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-14 13:00:53 +0100 (to, 14 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1192 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using WCell.Constants.Factions;
using WCell.Constants.GameObjects;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.RealmServer.Factions;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.GameObjects.Spawns;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Quests;

namespace WCell.RealmServer.Entities
{
	public partial class GameObject
	{
		public GossipMenu GossipMenu
		{
			get;
			set;
		}

		public GameObjectHandler Handler
		{
			get { return m_handler; }
			set
			{
				m_handler = value;
				m_handler.Initialize(this);
			}
		}

		public override string Name
		{
			get { return m_entry != null ? m_entry.DefaultName : ""; }
			set
			{
				throw new NotImplementedException("Dynamic renaming of GOs is not implementable.");
			}
		}

		public GOEntry Entry
		{
			get { return m_entry; }
		}

		public override ObjectTemplate Template
		{
			get { return Entry; }
		}

		/// <summary>
		/// The Template of this GO (if any was used)
		/// </summary>
		public GOSpawnPoint SpawnPoint
		{
			get { return m_spawnPoint; }
		}

		/// <summary>
		/// Traps get removed when their AreaAura gets removed
		/// </summary>
		public override bool IsTrap
		{
			get { return m_IsTrap; }
		}

		public EntityId CreatedBy
		{
			get { return GetEntityId(GameObjectFields.OBJECT_FIELD_CREATED_BY); }
			set { SetEntityId(GameObjectFields.OBJECT_FIELD_CREATED_BY, value); }
		}

		public uint DisplayId
		{
			get { return GetUInt32(GameObjectFields.DISPLAYID); }
			set { SetUInt32(GameObjectFields.DISPLAYID, value); }
		}

		public GameObjectFlags Flags
		{
			get { return (GameObjectFlags)GetUInt32(GameObjectFields.FLAGS); }
			set { SetUInt32(GameObjectFields.FLAGS, (uint)value); }
		}

		#region BYTES_1

		public bool IsEnabled
		{
			get { return (GameObjectState)GetByte(GameObjectFields.BYTES_1, 0) == GameObjectState.Enabled; }
			set { SetByte(GameObjectFields.BYTES_1, 0, (byte)(value ? GameObjectState.Enabled : GameObjectState.Disabled)); }
		}

		public GameObjectState State
		{
			get { return (GameObjectState)GetByte(GameObjectFields.BYTES_1, 0); }
			set { SetByte(GameObjectFields.BYTES_1, 0, (byte)value); }
		}

		public GameObjectType GOType
		{
			get { return (GameObjectType)GetByte(GameObjectFields.BYTES_1, 1); }
			set { SetByte(GameObjectFields.BYTES_1, 1, (byte)value); }
		}

		/// <summary>        
		/// No idea        
		/// </summary>        
		public byte ArtKit
		{
			get { return GetByte(GameObjectFields.BYTES_1, 2); }
			set { SetByte(GameObjectFields.BYTES_1, 2, value); }
		}

		/// <summary>        
		/// Seems to be 0 or 100 mostly        
		/// </summary>        
		public byte AnimationProgress
		{
			get { return GetByte(GameObjectFields.BYTES_1, 3); }
			set { SetByte(GameObjectFields.BYTES_1, 3, value); }
		}

		#endregion

		public byte[] Dynamic
		{
			get { return GetByteArray(GameObjectFields.DYNAMIC); }
			set { SetByteArray(GameObjectFields.DYNAMIC, value); }
		}

		public override Faction Faction
		{
			get
			{
				return m_faction;
			}
			set
			{
				m_faction = value;
				SetUInt32(GameObjectFields.FACTION, value.Template.Id);
			}
		}

		public override FactionId FactionId
		{
			get
			{
				return m_faction.Id;
			}
			set
			{
				var fac = FactionMgr.Get(value);
				if (fac != null)
				{
					Faction = fac;
				}
				else
				{
					SetUInt32(GameObjectFields.FACTION, (uint)value);
				}
			}
		}

		public int Level
		{
			get { return GetInt32(GameObjectFields.LEVEL); }
			set { SetInt32(GameObjectFields.LEVEL, value); }
		}

		public Unit Owner
		{
			get { return m_master; }
			set
			{
				Master = value;
				Faction = value != null ? value.Faction : Faction.NullFaction;
			}
		}

		#region Rotation
		public float ParentRotation1
		{
			get { return GetFloat(GameObjectFields.PARENTROTATION); }
			set { SetFloat(GameObjectFields.PARENTROTATION, value); }
		}

		public float ParentRotation2
		{
			get { return GetFloat(GameObjectFields.PARENTROTATION_2); }
			set { SetFloat(GameObjectFields.PARENTROTATION_2, value); }
		}

		public float ParentRotation3
		{
			get { return GetFloat(GameObjectFields.PARENTROTATION_3); }
			set { SetFloat(GameObjectFields.PARENTROTATION_3, value); }
		}

		public float ParentRotation4
		{
			get { return GetFloat(GameObjectFields.PARENTROTATION_4); }
			set { SetFloat(GameObjectFields.PARENTROTATION_4, value); }
		}

		public long Rotation
		{
			get;
			set;
		}

		private static readonly double RotatationConst = Math.Atan(Math.Pow(2.0f, -20.0f));

		protected void SetRotationFields(float[] rotations)
		{
			if (rotations.Length != 4)
				return;

			SetFloat(GameObjectFields.PARENTROTATION + 0, rotations[0]);
			SetFloat(GameObjectFields.PARENTROTATION + 1, rotations[1]);

			double rotSin = Math.Sin(Orientation / 2.0f),
				   rotCos = Math.Cos(Orientation / 2.0f);

			Rotation = (long)(rotSin / RotatationConst * (rotCos >= 0 ? 1.0f : -1.0f)) & 0x1FFFFF;

			if (rotations[2] == 0 && rotations[3] == 0)
			{
				SetFloat(GameObjectFields.PARENTROTATION + 2, (float)rotSin);
				SetFloat(GameObjectFields.PARENTROTATION + 3, (float)rotCos);
			}
			else
			{
				SetFloat(GameObjectFields.PARENTROTATION + 2, rotations[2]);
				SetFloat(GameObjectFields.PARENTROTATION + 3, rotations[3]);
			}
		}
		#endregion

		public override ObjectTypeCustom CustomType
		{
			get
			{
				return ObjectTypeCustom.Object | ObjectTypeCustom.GameObject;
			}
		}
	}
}