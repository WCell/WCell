using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Items;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.Util.Logging;
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
			var chr = (Character) target;
			if (Effect.Spell.Skill == null)
			{
				log.Warn("Spell {0} had Handler for Proficiency but Spell has no Skill associated with it.", Effect.Spell);
			}
			else if (!chr.Skills.Contains(Effect.Spell.Skill.Id))
			{
				chr.Skills.Add(Effect.Spell.Skill, false);
			}

			if (Effect.Spell.RequiredItemClass == ItemClass.Weapon)
			{
				chr.Skills.WeaponProficiency |= Effect.Spell.RequiredItemSubClassMask;
			}
			else if (Effect.Spell.RequiredItemClass == ItemClass.Armor)
			{
				chr.Skills.ArmorProficiency |= Effect.Spell.RequiredItemSubClassMask;
			}
		}

		public override ObjectTypes TargetType
		{
			get
			{
				return ObjectTypes.Player;
			}
		}
	}
}