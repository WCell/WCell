/*************************************************************************
 *
 *   file		: AuraEffectHandler.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-28 16:34:36 +0100 (to, 28 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1231 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras
{
	public class AuraVoidHandler : AuraEffectHandler
	{
	}

	/// <summary>
	/// An AuraEffectHandler handles the behavior of an aura
	/// </summary>
	public abstract class AuraEffectHandler
	{
		protected internal Aura m_aura;
		protected internal SpellEffect m_spellEffect;
		public int BaseEffectValue;

		public int EffectValue
		{
			get
			{
				if (m_aura != null &&
					m_aura.Spell.CanStack &&
					m_aura.StackCount > 1)
				{
					return BaseEffectValue * m_aura.StackCount;
				}
				return BaseEffectValue;
			}
		}

		public Unit Holder
		{
			get { return m_aura.Auras.Owner; }
		}

		protected internal void Init(Aura aura)
		{
			m_aura = aura;
		}

		/// <summary>		
		/// /// Check whether this handler can be applied to the given target
		///  </summary>
		protected internal virtual void CheckInitialize(CasterInfo casterInfo, Unit target, ref SpellFailedReason failReason)
		{
		}

		/// <summary>
		/// Applies this EffectHandler's effect to its holder
		/// </summary>
		protected internal virtual void Apply()
		{
		}

		/// <summary>
		/// Is called by Aura to remove the effect from its holder
		/// </summary>
		protected internal virtual void Remove(bool cancelled)
		{
		}

		/// <summary>
		/// whether this is a positive effect (by default: If they have a positive value)
		/// </summary>
		public virtual bool IsPositive
		{
			get { return EffectValue >= 0; }
		}

		/// <summary>
		/// The Aura to which this AuraEffect belongs
		/// </summary>
		public Aura Aura
		{
			get { return m_aura; }
		}

		public Unit Owner
		{
			get { return m_aura.Auras.Owner; }
		}

		/// <summary>
		/// The SpellEffect which triggered this AuraEffect
		/// </summary>
		public SpellEffect SpellEffect
		{
			get { return m_spellEffect; }
		}

		/// <summary>
		/// Indicates whether this aura is a stronger version of the given Aura 
		/// (not very accurate, can only be consistent for comparison of 2 Auras of the same type).
		/// </summary>
		public virtual bool IsStrongerThan(AuraEffectHandler otherHandler)
		{
			return m_spellEffect.BasePoints > otherHandler.m_spellEffect.BasePoints;
		}

		/// <summary>
		/// Returns whether the 2 Auras are equally strong. 
		/// If stronger or equally strong, reapplying non-stackable Auras will result into duration resets.
		/// </summary>
		public virtual bool IsEquallyStrong(AuraEffectHandler otherHandler)
		{
			return m_spellEffect.BasePoints == otherHandler.m_spellEffect.BasePoints;
		}

		/// <summary>
		/// Indicates whether this aura is a stronger version of the given Aura 
		/// (not very accurate, can only be consistent for comparison of 2 Auras of the same type).
		/// </summary>
		public virtual bool IsStrongerOrEqual(AuraEffectHandler otherHandler)
		{
			return IsEquallyStrong(otherHandler) || IsStrongerThan(otherHandler);
		}

		/// <summary>
		/// Triggers a proc on this EffectHandler with the given target.
		/// </summary>
		/// <param name="target"></param>
		public virtual void OnProc(Unit target, IUnitAction action)
		{
		}
	}
}