using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.NPCs;
using WCell.Util.Data;

namespace WCell.RealmServer.NPCs.Pets
{
	[DataHolder]
	public class PetLevelStatInfo : IDataHolder
	{
		public NPCId EntryId;
		public int Level;
		public int Health, Mana;
		public int Armor;
		public int Strength, Agility, Stamina, Intelligence, Spirit;

		public void FinalizeDataHolder()
		{
			
		}
	}
}
