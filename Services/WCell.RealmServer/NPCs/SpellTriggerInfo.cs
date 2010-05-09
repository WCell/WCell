using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Spells;
using WCell.Util.Data;
using WCell.Constants.Spells;

namespace WCell.RealmServer.NPCs
{
	public class SpellTriggerInfo
	{
		[NotPersistent]
		public Spell Spell;

		private SpellId m_SpellId;

		public SpellId SpellId
		{
			get { return m_SpellId; }
			set
			{
				m_SpellId = value;
				if (value != 0)
				{
					Spell = SpellHandler.Get(value);
				}
			}
		}

		public uint QuestId;
	}
}
