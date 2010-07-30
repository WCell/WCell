using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Core.Initialization;
using WCell.RealmServer.Spells;

namespace WCell.Addons.Default.MiscFixes
{
	public static class ShapeshiftFixes
	{
		[Initialization]
		public static void FixShapeshiftEntries()
		{
			SetPowerType(PowerType.Rage, ShapeshiftForm.Bear, ShapeshiftForm.DireBear);
			SetPowerType(PowerType.Energy, ShapeshiftForm.Cat);
		}

		static void SetPowerType(PowerType type, params ShapeshiftForm[] forms)
		{
			foreach (var form in forms)
			{
				SpellHandler.GetShapeshiftEntry(form).PowerType = type;
			}
		}
	}
}
