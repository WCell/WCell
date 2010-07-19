using System;
using WCell.Constants.World;
using WCell.Core.Database;
using WCell.RealmServer.Global;
using WCell.Util;

namespace WCell.RealmServer.Instances
{
	public class InstanceBinding : WCellRecord<InstanceBinding>, IRegionId
	{
		private int m_DifficultyIndex;

		public InstanceBinding(uint id, MapId mapId, uint difficultyIndex)
		{
			BindTime = DateTime.Now;
			DifficultyIndex = difficultyIndex;
			InstanceId = id;
			RegionId = mapId;
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

		public MapId RegionId
		{
			get;
			set;
		}

		public uint InstanceId
		{
			get;
			set;
		}

		public RegionInfo RegionInfo
		{
			get { return World.GetRegionInfo(RegionId); }
		}

		public MapDifficultyEntry Difficulty
		{
			get { return RegionInfo.Difficulties.Get(DifficultyIndex); }
		}

		/// <summary>
		/// Might Return null
		/// </summary>
		public InstancedRegion Instance
		{
			get { return World.GetInstance(RegionId, InstanceId); }
		}

		public DateTime NextResetTime
		{
			get { return InstanceMgr.GetNextResetTime(RegionId, DifficultyIndex); }
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
				left.RegionId == right.RegionId &&
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
			return obj.m_DifficultyIndex == m_DifficultyIndex &&
				obj.RegionId == RegionId &&
				obj.InstanceId == InstanceId;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (InstanceBinding)) return false;
			return Equals((InstanceBinding) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var result = m_DifficultyIndex;
				result = (result*397) ^ BindTime.GetHashCode();
				result = (result*397) ^ RegionId.GetHashCode();
				result = (result*397) ^ InstanceId.GetHashCode();
				return result;
			}
		}
		#endregion
	}
}