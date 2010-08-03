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

using WCell.Constants.Factions;
using WCell.Constants.GameObjects;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.RealmServer.Factions;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Quests;

namespace WCell.RealmServer.Entities
{
    public partial class GameObject
    {
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
				if (fac != null) {
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

		#region QuestGiver
		/// <summary>
		/// All available Quest information, in case that this is a QuestGiver
		/// </summary>
		public QuestHolderInfo QuestHolderInfo
		{
			get
			{
				// TODO:
				return null;
			}
		}

		public bool CanGiveQuestTo(Character chr)
		{
			return IsInRadiusSq(chr, GOMgr.DefaultInteractDistanceSq);
		}
		#endregion

    	public Unit Owner
    	{
    		get { return m_master; }
            set
            {
                Master = value;
                Faction = value != null ? value.Faction : Faction.NullFaction;
            }
    	}

		public long Rotation
		{
			get; set;
		}

    	public override ObjectTypeCustom CustomType
		{
			get
			{
				return ObjectTypeCustom.Object | ObjectTypeCustom.GameObject;
			}
		}
    }
}