using WCell.Constants;

namespace WCell.RealmServer.Battlegrounds
{
	public class InstanceBattlegroundQueue : BattlegroundQueue
	{
		private Battleground m_battleground;

		public InstanceBattlegroundQueue(Battleground bg)
		{
			m_battleground = bg;

			m_Template = bg.Template;
			m_MinLevel = bg.MinLevel;
			m_MaxLevel = bg.MaxLevel;
		}

		protected override BattlegroundTeamQueue CreateTeamQueue(BattlegroundSide side)
		{
			return new InstanceBGTeamQueue(this, side);
		}

		public override bool RequiresLocking
		{
			get { return false; }
		}

		public override Battleground Battleground
		{
			get { return m_battleground; }
		}

		protected internal override void Dispose()
		{
			base.Dispose();

			m_battleground = null;
		}
	}
}