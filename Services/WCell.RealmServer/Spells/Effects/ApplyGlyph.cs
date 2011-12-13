using WCell.Constants.Spells;
using WCell.RealmServer.Talents;

namespace WCell.RealmServer.Spells.Effects
{
	public class ApplyGlyphEffectHandler : SpellEffectHandler
	{ 
		public ApplyGlyphEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}
		public override SpellFailedReason Initialize()
		{
			if (m_cast.GlyphSlot != 0)
			{
				var glyph = (uint)m_cast.Spell.Effects[0].MiscValue;
				var properties = GlyphInfoHolder.GetPropertiesEntryForGlyph(glyph);
				var slot = GlyphInfoHolder.GetGlyphSlotEntryForGlyphSlotId(m_cast.CasterChar.GetGlyphSlot((byte)m_cast.GlyphSlot));
				if (properties.TypeFlags != slot.TypeFlags)
				{
					return SpellFailedReason.InvalidGlyph;
				}
			}

			return SpellFailedReason.Ok;
		}
		public override void Apply()
		{
			var chr = m_cast.CasterChar;
			chr.ApplyGlyph((byte)m_cast.GlyphSlot, GlyphInfoHolder.GetPropertiesEntryForGlyph((uint)m_cast.Spell.Effects[0].MiscValue));
		}
	}
}
