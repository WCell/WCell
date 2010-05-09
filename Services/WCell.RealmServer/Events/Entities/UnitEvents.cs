using System;
using WCell.Constants;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.Entities
{
	public delegate void UnitActionCallback(Unit unit);

    public partial class Unit
	{
		public delegate void ShapeShiftHandler(Unit unit, ShapeShiftForm oldForm);

		/// <summary>
		/// Is called when this Unit shapeshifts into another form
		/// </summary>
		public static event ShapeShiftHandler ShapeShiftChanged;

		public static event Action<Unit, SpellCast, Aura> Debuff;

		/// <summary>
		/// Called when this Unit got the given debuff by the given SpellCast
		/// </summary>
		/// <param name="cast"></param>
		internal void OnDebuff(SpellCast cast, Aura debuff)
		{
			// force combat mode
			IsInCombat = true;

			var evt = Debuff;
			if (evt != null)
			{
				evt(this, cast, debuff);
			}
		}
    }
}
