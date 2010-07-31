namespace WCell.RealmServer.Spells.Auras
{
	/// <summary>
	/// Represents Unit- and AreaAuras
	/// </summary>
	public interface IAura
	{
		int TimeLeft
		{
			get;
		}

		bool IsAdded
		{
			get;
		}

		/// <summary>
		/// Initializes the Aura. Depending on how the Aura is created,
		/// it might be called internally or has to be called explicitely.
		/// </summary>
		/// <param name="noTimeout">Whether the Aura should always continue and never expire.</param>
		void Start(ITickTimer controller, bool noTimeout);

		void Apply();

		void Remove(bool cancelled);
	}
}