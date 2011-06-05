using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras.Handlers;
using WCell.Constants;

namespace WCell.Addons.Default.Spells.Druid
{
	public static class DruidFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixSpells()
		{
			SpellLineId.DruidCatForm.Apply(spell =>
			{
				spell.GetEffect(AuraType.ModShapeshift).AuraEffectHandlerCreator = () => new TrackHumanoidsHandler();
			});
		}
	}

	public class TrackHumanoidsHandler : ShapeshiftHandler
	{
		protected override void Apply()
		{
			base.Apply();
		}

		protected override void Remove(bool cancelled)
		{
			if (Owner.Auras[SpellId.ClassSkillTrackHumanoids] != null)
			{
				Owner.Auras.Remove(SpellId.ClassSkillTrackHumanoids);
			}
			base.Remove(cancelled);
		}
	}
}