using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

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

		public override SpellFailedReason InitializeTarget(WorldObject target)
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

			var caster = m_cast.CasterUnit;
			if (caster != null)
			{
				npc.ThreatCollection[caster] +=
					caster.GetGeneratedThreat(CalcEffectValue(), Effect.Spell.Schools[0], Effect);
			}
		}

		public override ObjectTypes TargetType
		{
			get { return ObjectTypes.Unit; }
		}
	}
}