namespace WCell.RealmServer.Spells
{
    public interface ISpellParameters
    {
        Spell Spell { get; }

        int MaxCharges { get; }

        int Amplitude { get; }

        int StartDelay { get; }

        int Radius { get; }
    }
}