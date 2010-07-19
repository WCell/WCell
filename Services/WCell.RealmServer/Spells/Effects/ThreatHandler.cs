using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells.Effects
{
	/// <summary>
	/// Generates Threat
	/// </summary>
	public class ThreatHandler : SpellEffectHandler
	{
		public ThreatHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override SpellFailedReason CheckValidTarget(WorldObject target)
		{
			if (!(target is NPC))
			{
				return SpellFailedReason.DontReport;
			}
			return SpellFailedReason.Ok;
		}

		protected override void Apply(WorldObject target)
		{
			var npc = (NPC)target;

			npc.ThreatCollection[(Unit)m_cast.Caster] += 
				((Unit)m_cast.Caster).GetGeneratedThreat(CalcEffectValue(), Effect.Spell.Schools[0], Effect);
		}

		public override ObjectTypes CasterType
		{
			get { return ObjectTypes.Unit; }
		}

		public override ObjectTypes TargetType
		{
			get { return ObjectTypes.Unit; }
		}
	}
}