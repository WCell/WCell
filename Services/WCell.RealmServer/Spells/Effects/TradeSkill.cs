using WCell.Constants.Skills;

namespace WCell.RealmServer.Spells.Effects
{
	/// <summary>
	/// Learns a Trade Skill (add skill checks here?)
	/// </summary>
	public class TradeSkillEffectHandler : SpellEffectHandler
	{
		public TradeSkillEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override void Apply()
		{
			//Zero or -1
			var unk = Effect.BasePoints;
			//Zero or 1
			var unk2 = Effect.DiceSides;
		}

		public override bool HasOwnTargets
		{
			get { return false; }
		}
	}
}