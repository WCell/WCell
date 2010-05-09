using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.NPCs
{
	public class NPCCollection : List<NPC>
	{
		public NPCCollection() : base(3)
		{
			
		}
	}
}
