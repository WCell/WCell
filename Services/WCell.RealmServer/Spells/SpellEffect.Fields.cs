/*************************************************************************
 *
 *   file		: SpellEffect.DBC.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-09-15 13:15:29 +0800 (Tue, 15 Sep 2009) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1100 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants.Spells;
using WCell.RealmServer.Content;
using WCell.Util.Data;

namespace WCell.RealmServer.Spells
{
	public partial class SpellEffect
	{
		/* Summary of $ values
		 * $sX = MinValue to MaxValue
		 * $oX = Amplitude
		 * $eX = ProcValue
		 * $bX = PointsPerComboPoint
		 * */
		#region DBC Fields

		/// <summary>
		/// SpellEffectNames.dbc - no longer included in mpqs
		/// </summary>
		public SpellEffectType EffectType;

		/// <summary>
		/// Random value max (BaseDice to DiceSides)
		/// </summary>
		public int DiceSides;

		public float RealPointsPerLevel;

		/// <summary>
		/// Base value
		/// Value = BasePoints + rand(BaseDice, DiceSides)
		/// </summary>
		public int BasePoints;

		public SpellMechanic Mechanic;

		public ImplicitTargetType ImplicitTargetA;
		public ImplicitTargetType ImplicitTargetB;

		/// <summary>
		/// SpellRadius.dbc
		/// Is always at least 5y. 
		/// If area-related spells dont have a radius we just look for very close targets
		/// </summary>
		public float Radius;

		/// <summary>
		/// SpellAuraNames.dbc - no longer included in mpqs
		/// </summary>
		public AuraType AuraType;

		/// <summary>
		/// Interval-delay in milliseconds
		/// </summary>
		public int Amplitude;

		public int GetMaxTicks()
		{
			return Spell.Durations.Max/Amplitude;
		}

		/// <summary>
		/// $e1/2/3 in Description
		/// </summary>
		public float ProcValue;

		public int ChainTargets;

		/// <summary>
		/// 
		/// </summary>
		public uint ItemId;

		public int MiscValue;

		public int MiscValueB;

		/// <summary>
		/// Not set during InitializationPass 2, so 
		/// for fixing things, use GetTriggerSpell() instead.
		/// </summary>
		[NotPersistent]
		public Spell TriggerSpell;
		public SpellId TriggerSpellId;

		public Spell GetTriggerSpell()
		{
			var spell = SpellHandler.Get(TriggerSpellId);
			if (spell == null && ContentHandler.ForceDataPresence)
			{
				throw new ContentException("Spell {0} does not have a valid TriggerSpellId: {1}", this, TriggerSpellId);
			}
			return spell;
		}

		/// <summary>
		/// 
		/// </summary>
		public float PointsPerComboPoint;

		/// <summary>
		/// Multi purpose.
		/// 1. If it is a proc effect, determines set of spells that can proc this proc (use <see cref="AddToAffectMask"/>)
		/// 2. If it is a modifier effect, determines set of spells to be affected by this effect
		/// 3. Ignored in some cases
		/// 4. Special applications in some cases
		/// </summary>
		[Persistent(3)]
		public uint[] AffectMask = new uint[3];
		#endregion

		#region IDataHolder Members

		//void IDataHolder.FinalizeAfterLoad()
		//{
		//    Initialize();
		//    Init2();
		//    if (Spell != null)
		//    {
		//        ArrayUtil.EnsureSize(ref Spell.Effects, EffectIndex);
		//        Spell.Effects[EffectIndex] = this;
		//        return;
		//    }
		//}

		#endregion
	}
}