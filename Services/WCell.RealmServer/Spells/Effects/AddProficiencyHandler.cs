using NLog;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.Spells.Effects
{
	public class AddProficiencyHandler : SpellEffectHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public AddProficiencyHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		protected override void Apply(WorldObject target)
		{
			var chr = (Character)target;

		    ItemClass requiredItemClass = Effect.Spell.SpellEquippedItems != null ? Effect.Spell.SpellEquippedItems.RequiredItemClass : ItemClass.None;
            //Effect.Spell.SpellEquippedItems is implicitly non null if requiredItemClass != ItemClass.None; see the above line;
            if (requiredItemClass == ItemClass.Weapon && !chr.Skills.WeaponProficiency.HasAnyFlag(Effect.Spell.SpellEquippedItems.RequiredItemSubClassMask))
			{
                chr.Skills.WeaponProficiency |= Effect.Spell.SpellEquippedItems.RequiredItemSubClassMask;
				CharacterHandler.SendProficiency(chr, ItemClass.Weapon, chr.Skills.WeaponProficiency);
				
			}
            else if (requiredItemClass == ItemClass.Armor && !chr.Skills.ArmorProficiency.HasAnyFlag(Effect.Spell.SpellEquippedItems.RequiredItemSubClassMask))
			{
                chr.Skills.ArmorProficiency |= Effect.Spell.SpellEquippedItems.RequiredItemSubClassMask;
				CharacterHandler.SendProficiency(chr, ItemClass.Armor, chr.Skills.ArmorProficiency);
			}

			if (Effect.Spell.Ability == null)
			{
				log.Warn("Spell {0} had Handler for Proficiency but Spell has no Skill associated with it.", Effect.Spell);
			}
			else if (!chr.Skills.Contains(Effect.Spell.Ability.Skill.Id))
			{
				chr.Skills.Add(Effect.Spell.Ability.Skill, false);
			}


		}

		public override ObjectTypes TargetType
		{
			get { return ObjectTypes.Player; }
		}
	}
}