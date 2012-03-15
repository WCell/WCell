using System;
using Castle.ActiveRecord;
using WCell.Constants.World;
using WCell.Core.Database;
using WCell.RealmServer.Global;
using WCell.Util;

namespace WCell.RealmServer.Instances
{
    /// <summary>
    /// Defines the progress inside an instance that can also be saved to DB
    /// </summary>
    [ActiveRecord(Access = PropertyAccess.Property)]
    public class InstanceProgress : WCellRecord<InstanceProgress>, IMapId
    {
        [Field("InstanceId", NotNull = true, Access = PropertyAccess.Field)]
        private int _instanceId;

        public InstanceProgress()
        {
            MapId = MapId.None;
            InstanceId = 0;
        }

        public InstanceProgress(MapId mapId, uint instanceId)
        {
            MapId = mapId;
            InstanceId = instanceId;
            State = RecordState.New;
        }

        [PrimaryKey(PrimaryKeyType.Assigned)]
        public long Guid
        {
            get { return Utility.MakeLong((int)MapId, _instanceId); }
            set
            {
                int mapId = 0;
                Utility.UnpackLong(value, ref mapId, ref _instanceId);
                MapId = (MapId)mapId;
            }
        }

        [Property(NotNull = true)]
        public MapId MapId
        {
            get;
            set;
        }

        public uint InstanceId
        {
            get { return (uint)_instanceId; }
            set { _instanceId = (int)value; }
        }

        [Property(NotNull = true)]
        public uint DifficultyIndex
        {
            get;
            set;
        }

        [Property]
        public DateTime ResetTime
        {
            get;
            set;
        }

        [Property]
        /// <summary>
        /// The version number, so we know how CustomData looks like, even if we changed the layout
        /// </summary>
        public int CustomDataVersion
        {
            get;
            set;
        }

        [Property]
        /// <summary>
        /// Instance-dependent data: Many instances need custom data saved to DB, they can use this field for that.
        /// TODO: Use SerializerCollection to make this safe to use
        /// </summary>
        public byte[] CustomData
        {
            get;
            set;
        }
    }
}
