using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Terrain.MPQ
{
	[Flags]
	public enum WDTFlags
	{
		GlobalWMO = 1,
	}
	/// <summary>
	/// MPHD chunk
	/// </summary>
	public class WDTHeader
	{
		public WDTFlags Header1;
		public int Header2;
		public int Header3;
		public int Header4;
		public int Header5;
		public int Header6;
		public int Header7;
		public int Header8;

		public bool IsWMOMap
		{
			get { return (Header1 & WDTFlags.GlobalWMO) != 0; }
		}
	}
}
