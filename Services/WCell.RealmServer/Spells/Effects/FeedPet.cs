using WCell.Constants.NPCs;
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

		public override void Initialize(ref SpellFailedReason failReason)
		{
			var pet = Cast.CasterChar.ActivePet;

			if (pet == null)
			{
				failReason = SpellFailedReason.BadImplicitTargets;
				return;
			}

			if (Cast.UsedItem == null)
			{
				failReason = SpellFailedReason.ItemNotFound;
				return;
			}

			var food = Cast.UsedItem.Template;

			if (!pet.CanEat(food.m_PetFood))
			{
				failReason = SpellFailedReason.WrongPetFood;
				return;
			}

			var diff = pet.Level - food.Level;
			if (diff > 35)
			{
				failReason = SpellFailedReason.FoodLowlevel;
				return;
			}

			if (diff < -15)
			{
				failReason = SpellFailedReason.Highlevel;
				return;
			}
		}

		public override void Apply()
		{
			var pet = Cast.CasterChar.ActivePet;
			var food = Cast.UsedItem.Template;

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