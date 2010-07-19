using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.Talents
{
	public class TalentSpec
	{
		public List<ITalentRecord> Talents;
	}
	
	public class CharacterTalentSpec : TalentSpec
	{
		public List<GlyphRecord> Glyphs;

		public byte[] ActionBar;
	}
}