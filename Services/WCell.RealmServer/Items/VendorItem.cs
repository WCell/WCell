using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;

namespace WCell.RealmServer.Items
{
	public class VendorItem
	{
		public ItemTemplate Template = null;
		private uint numStacksForSale;
		private readonly uint maxStacksForSale;
		public readonly uint BuyStackSize;
		private DateTime lastUpdate;
		private readonly uint regenTime;

		/// <summary>
		/// If this item has a limited supply available, this returns a number smaller than uint.MaxValue
		/// </summary>
		public uint NumStacksForSale
		{
			get
			{
				if( numStacksForSale < uint.MaxValue ) 
				{
					uint numRegenedStacks = (uint)(( DateTime.Now - lastUpdate ).TotalSeconds / regenTime);
					if( ( numStacksForSale + numRegenedStacks ) > maxStacksForSale )
						numStacksForSale = maxStacksForSale;
					else
						numStacksForSale += numRegenedStacks;

					lastUpdate = DateTime.Now;
					return numStacksForSale;
				}
				return uint.MaxValue;
			}
			set
			{
				if( numStacksForSale < uint.MaxValue )
					numStacksForSale = Math.Max( value, 0 );
			}
		}

		public bool CanSelltoChar( Character character )
		{
			if( ( ( Template.RequiredClassMask & character.ClassMask ) == 0 ) &&
				( Template.BondType == ItemBondType.OnPickup ) &&
				( !character.IsGameMaster ) )
				return false;
			return true;
		}
		
		public uint GetPriceForStandingLevel( StandingLevel lvl )
		{
			return (uint)Math.Floor( Template.SellPrice * ( 1 - Reputation.GetReputationDiscount( lvl ) ) );
		}

		/// <param name="template">The ItemTemplate for this item.</param>
		/// <param name="numStacksForSale">The maximum number of lots of this item the vendor can sell per period of time. 0xFFFFFFFF means infinite.</param>
		/// <param name="buyStackSize">The vendor sells these items in lots of buyStackSize.</param>
		/// <param name="regenTime">If the vendor has a limited number of this item to sell, this is the time it takes to regen one item.</param>
		public VendorItem( ItemTemplate template, uint numStacksForSale, uint buyStackSize, uint regenTime )
		{
			Template = template;
			this.numStacksForSale = numStacksForSale;
			maxStacksForSale = numStacksForSale;
			BuyStackSize = buyStackSize;

			lastUpdate = DateTime.Now;
			this.regenTime = regenTime;
		}
	}
}