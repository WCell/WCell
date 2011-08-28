using System;
using Castle.ActiveRecord;
using WCell.Constants.World;
using WCell.Core.Database;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Instances
{
	/// <summary>
	/// Defines the progress inside an instance that can also be saved to DB
	/// </summary>
	public class InstanceProgress : WCellRecord<InstanceProgress>, IMapId
	{
		[Field("InstanceId", NotNull = true, Access = PropertyAccess.Field)]
		private int m_InstanceId;

		public InstanceProgress(MapId mapId, uint instanceId)
		{
			MapId = mapId;
			InstanceId = instanceId;
		}

		[Property(NotNull = true)]
		public MapId MapId
		{
			get;
			set;
		}

		public uint InstanceId
		{
			get { return (uint) m_InstanceId; }
			set { m_InstanceId = (int) value; }
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
