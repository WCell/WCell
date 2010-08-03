using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects.Custom
{
	/// <summary>
	/// Removes the cooldown for the SpellEffect.AffectSpellSet
	/// </summary>
	public class RemoveCooldownEffectHandler : SpellEffectHandler
	{
		public RemoveCooldownEffectHandler(SpellCast cast, SpellEffect effect) : base(cast, effect)
		{
		}

		public override void Initialize(ref SpellFailedReason failReason)
		{
			if (Effect.AffectSpellSet == null)
			{
				failReason = SpellFailedReason.Error;
				LogManager.GetCurrentClassLogger().Warn("Tried to use {0} in Spell \"{1}\" with an empty SpellEffect.AffectSpellSet", GetType(), Effect.Spell);
			}
		}

		protected override void Apply(WorldObject target)
		{
			if (target is Unit)
			{
				foreach (var spell in Effect.AffectSpellSet)
				{
					((Unit)target).Spells.ClearCooldown(spell, false);
				}
			}
		}
	}
}
