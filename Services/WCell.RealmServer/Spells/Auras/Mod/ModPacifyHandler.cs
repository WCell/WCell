using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells.Auras.Mod
{
	/// <summary>
	/// Prevents carrier from attacking or using "physical abilities"
	/// </summary>
	public class ModPacifyHandler : AuraEffectHandler
	{
		protected override void Apply()
		{
			Owner.Pacified++;
		}

		protected override void Remove(bool cancelled)
		{
			Owner.Pacified--;
		}
	}

	/// <summary>
	/// Prevents carrier from attacking or using "physical abilities"
	/// </summary>
	public class ModSilenceHandler : AuraEffectHandler
	{
		protected override void Apply()
		{
			Owner.IncMechanicCount(SpellMechanic.Silenced);
		}

		protected override void Remove(bool cancelled)
		{
			Owner.DecMechanicCount(SpellMechanic.Silenced);
		}
	}

	/// <summary>
	/// Prevents carrier from attacking or using "physical abilities"
	/// </summary>
	public class ModPacifySilenceHandler : AuraEffectHandler
	{
		protected override void Apply()
		{
			Owner.Pacified++;
			Owner.IncMechanicCount(SpellMechanic.Silenced);
		}

		protected override void Remove(bool cancelled)
		{
			Owner.Pacified--;
			Owner.DecMechanicCount(SpellMechanic.Silenced);
		}
	}
}