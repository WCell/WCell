using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.Instances
{
	public abstract class InstanceSettings
	{
		protected InstanceSettings(BaseInstance instance)
		{
			Instance = instance;
		}

		public BaseInstance Instance
		{
			get;
			private set;
		}
	}
}