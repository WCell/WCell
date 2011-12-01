using System;
using Castle.ActiveRecord;
using WCell.Constants.World;
using WCell.Core.Database;
using WCell.RealmServer.Global;
using WCell.Util;

namespace WCell.RealmServer.Instances
{
    [ActiveRecord(Access = PropertyAccess.Property)]
	public class InstanceBinding : WCellRecord<InstanceBinding>, IMapId
	{
		private int _difficultyIndex;

        public InstanceBinding()
        {
            BindTime = DateTime.Now;
            DifficultyIndex = 0;
            InstanceId = 0;
            MapId = MapId.None;
        }

		public InstanceBinding(uint id, MapId mapId, uint difficultyIndex)
		{
			BindTime = DateTime.Now;
			DifficultyIndex = difficultyIndex;
			InstanceId = id;
			MapId = mapId;
		}

		public uint InstanceId
		{
			get { return (uint)_instanceId; }
			set { _instanceId = (int)value; }
		}

        [PrimaryKey]
        public long Guid
        {
            get { return Utility.MakeLong((int) MapId, _instanceId); }
            set
            {
                int mapId = 0;
                Utility.UnpackLong(value, ref mapId, ref _instanceId);
                MapId = (MapId) mapId;
            }
        }

        public MapId MapId { get; set; }

        private int _instanceId;

		[Property(NotNull = true)]
		public uint DifficultyIndex
		{
			get { return (uint)_difficultyIndex; }
			set { _difficultyIndex = (int)value; }
		}

		[Property(NotNull = true)]
		public DateTime BindTime
		{
			get;
			set;
		}

		public DateTime NextResetTime
		{
			get
			{
				// TODO: Correct reset time
				return InstanceMgr.GetNextResetTime(Difficulty);
			}
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
			get { return InstanceMgr.Instances.GetInstance(MapId, InstanceId); }
		}

		#region Misc Overrides
		public static bool operator ==(InstanceBinding left, InstanceBinding right)
		{
			if (ReferenceEquals(left, null))
			{
				return ReferenceEquals(right, null);
			}
			return !ReferenceEquals(right, null) &&
				left._difficultyIndex == right._difficultyIndex &&
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
			return obj._difficultyIndex == _difficultyIndex && obj.BindTime.Equals(BindTime) && Equals(obj.MapId, MapId) && obj.InstanceId == InstanceId;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (!(obj is InstanceBinding)) return false;
			return Equals((InstanceBinding)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var result = _difficultyIndex;
				result = (result * 397) ^ BindTime.GetHashCode();
				result = (result * 397) ^ MapId.GetHashCode();
				result = (result * 397) ^ InstanceId.GetHashCode();
				return result;
			}
		}
		#endregion
	}
}