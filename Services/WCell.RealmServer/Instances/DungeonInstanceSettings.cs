using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.Instances
{
	/// <summary>
	/// Used to "flavor" Dungeon instances
	/// </summary>
	public class DungeonInstanceSettings : InstanceSettings
	{
		protected DungeonInstanceSettings(BaseInstance instance) : base(instance)
		{
		}
	}
}
