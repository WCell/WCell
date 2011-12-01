using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
	/// <summary>
	/// TODO: Stop movement for a short time or until someting happened to the NPC
	/// </summary>
	public class DistractEffectHandler : SpellEffectHandler
	{
		public DistractEffectHandler(SpellCast cast, SpellEffect effect) : base(cast, effect)
		{
		}

		protected override void Apply(WorldObject target)
		{
			if (target is NPC)
			{
				var npc = (NPC)target;
				var millis = 1000*CalcEffectValue();

				npc.Face(m_cast.TargetLoc);
				npc.Movement.Stop();
			}
		}

		public override ObjectTypes TargetType
		{
			get
			{
				return ObjectTypes.Unit;
			}
		}
	}
}