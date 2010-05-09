using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;

namespace WCell.Addons.Default.Battlegrounds.WarsongGulch
{
	/// <summary>
	/// Belongs to the Spell to be casted when Flag is dropped
	/// </summary>
	public class SummonFlagEffectHandler : SpellEffectHandler
	{
		public SummonFlagEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override void Apply()
		{
			var chr = (Character)m_cast.CasterUnit;
			var wsg = chr.Region as WarsongGulch;
			if (wsg != null)
			{
				wsg.GetFaction(chr.Battlegrounds.Team.Side).Opponent.SummonDroppedFlag(chr);
			}
		}

		public override ObjectTypes CasterType
		{
			get { return ObjectTypes.Player; }
		}
	}
}