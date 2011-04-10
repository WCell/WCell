using WCell.RealmServer.Entities;
using WCell.RealmServer.Talents;
using WCell.RealmServer.Spells;
using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells.Effects
{
	public class ApplyGlyphEffectHandler : SpellEffectHandler
	{
		public ApplyGlyphEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}
		public override SpellFailedReason InitializeTarget(WorldObject target)
		{
			if (m_cast.m_glyphSlot != 0)
			{
				var glyph = (uint)m_cast.Spell.Effects[0].MiscValue;
				var properties = GlyphInfoHolder.GetPropertiesEntryForGlyph(glyph);
				var slot = GlyphInfoHolder.GetGlyphSlotEntryForGlyphSlotId(m_cast.CasterChar.GetGlyphSlot((byte)m_cast.m_glyphSlot));
				if (properties.TypeFlags != slot.TypeFlags)
				{
					return SpellFailedReason.InvalidGlyph;
				}

			}
			return SpellFailedReason.Ok;
		}
		protected override void Apply(WorldObject target)
		{
			//TODO: Apply the glyph
		}
	}
}
