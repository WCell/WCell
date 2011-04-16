using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.NPCs
{
	public enum VendorInventoryError
	{
		/// <summary>
		/// Vendor has no inventory
		/// </summary>
		NoInventory = 0,

		/// <summary>
		/// I don't think he likes you very much
		/// </summary>
		BadRep,

		/// <summary>
		/// You are too far away
		/// </summary>
		TooFarAway,

		/// <summary>
		/// Vendor is dead
		/// </summary>
		VendorDead,

		/// <summary>
		/// You can't shop while dead
		/// </summary>
		YouDead
	}
}