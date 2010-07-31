using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AreaTriggers
{
	public partial class AreaTrigger
	{
		public delegate void ATUseHandler(AreaTrigger at, Character triggerer);

		public event ATUseHandler Triggered;
	}
}