using WCell.Constants.Spells;
using WCell.Constants.Updates;

namespace WCell.RealmServer.Spells.Effects
{
	/// <summary>
	/// Is used to feed the currently ActivePet of the Caster
	/// </summary>
	public class FeedPetEffectHandler : SpellEffectHandler
	{
		public FeedPetEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override bool HasOwnTargets
		{
			get { return false; }
		}

		public override ObjectTypes CasterType
		{
			get { return ObjectTypes.Player; }
		}

		public override SpellFailedReason Initialize()
		{
			var pet = Cast.CasterChar.ActivePet;

			if (pet == null)
			{
				return SpellFailedReason.BadImplicitTargets;
			}

			if (Cast.TargetItem == null)
			{
				return SpellFailedReason.ItemNotFound;
			}

			var food = Cast.TargetItem.Template;

			if (!pet.CanEat(food.m_PetFood))
			{
				return SpellFailedReason.WrongPetFood;
			}

			var diff = pet.Level - food.Level;
			if (diff > 35)
			{
				return SpellFailedReason.FoodLowlevel;
			}

			if (diff < -15)
			{
				return SpellFailedReason.Highlevel;
			}

			return SpellFailedReason.Ok;
		}

		public override void Apply()
		{
			var pet = Cast.CasterChar.ActivePet;
			var food = Cast.TargetItem.Template;

			if (pet == null || food == null)
			{
				return;
			}

			Cast.Trigger(Effect.TriggerSpell, pet);

			var petAura = pet.Auras[Effect.TriggerSpellId];
			if (petAura == null) return;

			var handler = petAura.GetHandler(AuraType.PeriodicEnergize);
			if (handler == null) return;

			handler.BaseEffectValue = pet.GetHappinessGain(food);
		}
	} // end class
}