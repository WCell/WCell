namespace WCell.RealmServer.Spells
{
	public interface ISpellParameters
	{
		Spell Spell { get; }

		uint MaxCharges { get; }

		int Amplitude { get; }

		uint StartDelay { get; }

		uint Radius { get; }
	}
}