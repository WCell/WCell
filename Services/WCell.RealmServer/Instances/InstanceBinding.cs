using System;
using WCell.Constants.World;
using WCell.Core.Database;
using WCell.RealmServer.Global;
using WCell.Util;

namespace WCell.RealmServer.Instances
{
	public class InstanceBinding : WCellRecord<InstanceBinding>, IMapId
	{
		private int m_DifficultyIndex;

		public InstanceBinding(uint id, MapId mapId, uint difficultyIndex)
		{
			BindTime = DateTime.Now;
			DifficultyIndex = difficultyIndex;
			InstanceId = id;
			MapId = mapId;
		}

		public DateTime BindTime
		{
			get;
			set;
		}

		public uint DifficultyIndex
		{
			get { return (uint)m_DifficultyIndex; }
			set { m_DifficultyIndex = (int)value; }
		}

		public MapId MapId
		{
			get;
			set;
		}

		public uint InstanceId
		{
			get;
			set;
		}

		public MapTemplate MapTemplate
		{
			get { return World.GetMapTemplate(MapId); }
		}

		public MapDifficultyEntry Difficulty
		{
			get { return MapTemplate.Difficulties.Get(DifficultyIndex); }
		}

		/// <summary>
		/// Might Return null
		/// </summary>
		public InstancedMap Instance
		{
			get { return World.GetInstance(MapId, InstanceId); }
		}

		public DateTime NextResetTime
		{
			get { return InstanceMgr.GetNextResetTime(MapId, DifficultyIndex); }
		}

		#region Misc Overrides
		public static bool operator ==(InstanceBinding left, InstanceBinding right)
		{
			if (ReferenceEquals(left, null))
			{
				return ReferenceEquals(right, null);
			}
			return !ReferenceEquals(right, null) &&
				left.m_DifficultyIndex == right.m_DifficultyIndex &&
				left.MapId == right.MapId &&
				left.InstanceId == right.InstanceId;
		}

		public static bool operator !=(InstanceBinding left, InstanceBinding right)
		{
			return !(left == right);
		}

		public bool Equals(InstanceBinding obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj.m_DifficultyIndex == m_DifficultyIndex && obj.BindTime.Equals(BindTime) && Equals(obj.MapId, MapId) && obj.InstanceId == InstanceId;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (!(obj is InstanceBinding)) return false;
			return Equals((InstanceBinding) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var result = m_DifficultyIndex;
				result = (result*397) ^ BindTime.GetHashCode();
				result = (result*397) ^ MapId.GetHashCode();
				result = (result*397) ^ InstanceId.GetHashCode();
				return result;
			}
		}
		#endregion
	}
}