namespace WCell.RealmServer.Battlegrounds
{
    public interface IBattlegroundRange
    {
        int MinLevel { get; }

        int MaxLevel { get; }

        BattlegroundTemplate Template { get; }
    }
}