using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.RealmServer.Lang;
using WCell.Util.Data;

namespace WCell.RealmServer.Global
{
	/// <summary>
	/// TODO: Load from game_event table
	/// </summary>
	public class WorldEvent //: IDataHolder
	{
		public uint Id;

		/// <summary>
		/// All translations of this event's name
		/// </summary>
		public string[] Names = new string[(int)ClientLocale.End];

		public string DefaultName
		{
			get { return Names != null ? Names.LocalizeWithDefaultLocale() : "[unknown Event]"; }
		}

		public DateTime From;
		public DateTime Until;
	}
}
