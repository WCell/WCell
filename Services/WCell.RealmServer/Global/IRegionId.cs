using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.World;

namespace WCell.RealmServer.Global
{
	public interface IRegionId
	{
		MapId RegionId { get; }

		uint InstanceId { get; }
	}
}