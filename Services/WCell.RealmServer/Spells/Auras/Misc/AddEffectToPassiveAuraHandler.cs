namespace WCell.RealmServer.Spells.Auras.Misc
{
	/// <summary>
	/// Adds an extra effect to a passive Aura
	/// </summary>
	public class AddEffectToPassiveAuraHandler : AuraEffectHandler
	{
		public SpellEffect ExtraEffect { get; set; }

		public AddEffectToPassiveAuraHandler()
		{
		}

		protected override void Apply()
		{
			var auras = m_aura.Auras as PlayerAuraCollection;
			if (auras != null)
			{
				
			}
		}

		protected override void Remove(bool cancelled)
		{
			
		}
	}
}
