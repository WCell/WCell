using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Global
{
	public partial class ZoneInfo
	{
		public delegate void ZonePlayerEnteredHandler(Character targetChr, Zone oldZone);
		public delegate void ZonePlayerLeftHandler(Character targetChr, Zone newZone);

		public event ZonePlayerEnteredHandler PlayerEntered;
		public event ZonePlayerLeftHandler PlayerLeft;
	}
}
