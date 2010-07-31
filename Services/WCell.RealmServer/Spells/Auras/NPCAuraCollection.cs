using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras
{
	public class NPCAuraCollection : AuraCollection
	{
		public NPCAuraCollection(NPC owner) : base(owner)
		{
		}
	}
}