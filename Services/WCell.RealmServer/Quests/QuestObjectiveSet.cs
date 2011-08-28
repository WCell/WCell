using WCell.Util.Data;

namespace WCell.RealmServer.Quests
{
	public class QuestObjectiveSet
	{
		[NotPersistent]
		public string[] Texts = new string[4];

		public string Text1
		{
			get { return Texts[0]; }
			set { Texts[0] = value; }
		}

		public string Text2
		{
			get { return Texts[1]; }
			set { Texts[1] = value; }
		}

		public string Text3
		{
			get { return Texts[2]; }
			set { Texts[2] = value; }
		}

		public string Text4
		{
			get { return Texts[3]; }
			set { Texts[3] = value; }
		}
	}
}