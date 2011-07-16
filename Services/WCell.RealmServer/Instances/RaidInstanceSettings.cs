using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.Instances
{
	/// <summary>
	/// Used to "flavor" Raid instances
	/// </summary>
	public class RaidInstanceSettings : InstanceSettings
	{
		public RaidInstanceSettings(BaseInstance instance)
			: base(instance)
		{
		}
	}
}
