using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Auras.Handlers;
using WCell.Util.Graphics;

namespace WCell.Addons.Default.Spells
{
	public static class MiscSpells
	{
		/// <summary>
		/// Some Spells need special handling
		/// </summary>
		[Initialization(InitializationPass.Second)]
		public static void Fix()
		{
			// Vehicle Spells need to send the vehicle as target and apply the vehicle aura to the caster
			SpellHandler.Apply(spell =>
			{
				spell.TargetFlags = SpellTargetFlags.Unit;
				var eff0 = spell.Effects[0];
				spell.Effects[0] = spell.Effects[1];
				spell.Effects[1] = eff0;
			}, SpellId.EnragedMammoth);

			// Deserter needs a custom AuraEffectHandler
			SpellHandler.Apply(spell =>
			{
				spell.Durations.Min = spell.Durations.Max = 15 * 60 * 1000;
				spell.Effects[0].AuraEffectHandlerCreator = () => new AuraDeserterHandler();
			}, SpellId.Deserter);

            //SpellHandler.Apply(spell =>
            //                       {
            //                           spell.Effects[0].
            //                       });
		}

		public class AuraDeserterHandler : AuraEffectHandler
		{
			protected override void Apply()
			{
				var chr = m_aura.Auras.Owner as Character;
				if (chr != null)
				{
					chr.Battlegrounds.IsDeserter = true;
				}
			}

			protected override void Remove(bool cancelled)
			{
				var chr = m_aura.Auras.Owner as Character;
				if (chr != null)
				{
					chr.Battlegrounds.IsDeserter = false;
				}
			}
		}
	}
}
