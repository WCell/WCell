/*************************************************************************
 *
 *   file		: SpellEffectHandler.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-28 05:18:24 +0100 (to, 28 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1229 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using NLog;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells.Auras;
using WCell.Util;

namespace WCell.RealmServer.Spells
{
	/// <summary>
	/// A SpellEffectHandler handles one SpellEffect during a SpellCast. 
	/// Supplied Caster and Target arguments will be checked against the Handler's CasterType and TargetType 
	/// properties before the Application of the Effect begins.
	/// The following methods will be called after another, on each of a Spell's SpellEffects:
	/// Before starting:
	/// 1. Init - Initializes all targets (by default adds standard targets) and checks whether this effect can succeed
	/// When performing:
	/// 2. CheckValidTarget - Checks whether this effect may be applied upon the given target
	/// 3. Apply - If none of the effects failed, applies the effect (by default to all targets)
	/// After performing:
	/// 4. Cleanup - Cleans up everything that is not wanted anymore
	/// </summary>
	public abstract class SpellEffectHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public readonly SpellEffect Effect;
		protected SpellCast m_cast;
		internal protected SpellTargetCollection Targets;
		private int CurrentTargetNo;

		protected SpellEffectHandler(SpellCast cast, SpellEffect effect)
		{
			m_cast = cast;
			Effect = effect;
		}

		#region Properties
		public SpellCast Cast
		{
			get { return m_cast; }
		}

		/// <summary>
		/// whether Targets need to be initialized
		/// </summary>
		public virtual bool HasOwnTargets
		{
			get { return Effect.HasTargets; }
		}

		/// <summary>
		/// The required Type for the Caster
		/// </summary>
		public virtual ObjectTypes CasterType
		{
			get { return ObjectTypes.None; }
		}

		/// <summary>
		/// The required Type for all Targets
		/// </summary>
		public virtual ObjectTypes TargetType
		{
			get { return ObjectTypes.All; }
		}
		#endregion

		/// <summary>
		/// Initializes this effect and checks whether the effect can be casted before Targets have been initialized.
		/// Use CheckValidTarget to validate Targets.
		/// </summary>
		public virtual void Initialize(ref SpellFailedReason failReason) { }

		/// <summary>
		/// This method is called on every target during CheckApply(). 
		/// Invalid targets either lead to Spell-Fail or the target being removed from Target-List.
		/// </summary>
		/// <returns>whether the given target is valid.</returns>
		public virtual SpellFailedReason InitializeTarget(WorldObject target)
		{
			return SpellFailedReason.Ok;
		}

		/// <summary>
		/// Apply the effect (by default to all targets of the targettype)
		/// Returns the reason for why it went wrong or SpellFailedReason.None
		/// </summary>
		public virtual void Apply()
		{
			if (Targets != null)
			{
				for (CurrentTargetNo = 0; CurrentTargetNo < Targets.Count; CurrentTargetNo++)
				{
					var target = Targets[CurrentTargetNo];
					if (!target.IsInContext)
					{
						continue;
					}
					Apply(target);
					if (m_cast == null)
					{
						// spell got cancelled
						return;
					}
				}
			}
			else
			{
				log.Warn("SpellEffectHandler has no targets, but Apply() is not overridden: " + this);
			}
		}

		// needed?
		#region Channeling

		protected internal void OnChannelTick()
		{
			Apply();
		}

		protected internal void OnChannelClose(bool cancelled)
		{
			Cleanup();
		}

		#endregion

		#region Protected
		/// <summary>
		/// Apply the effect to a single target
		/// </summary>
		/// <param name="target"></param>
		protected virtual void Apply(WorldObject target) { }

		/// <summary>
		/// Cleans up (if there is anything to clean)
		/// </summary>
		internal protected virtual void Cleanup()
		{
			m_cast = null;
			if (Targets != null)
			{
				Targets.Dispose();
				Targets = null;
			}
		}

		/// <summary>
		/// Called automatically after Effect creation, to check for valid caster type: 
		/// If invalid, a developer allowed a spell to be casted from the wrong context (or not?)
		/// </summary>
		protected internal void CheckCasterType(ref SpellFailedReason failReason)
		{
			if (CasterType != ObjectTypes.None && (m_cast.CasterObject == null || !m_cast.CasterObject.CheckObjType(CasterType)))
			{
				failReason = SpellFailedReason.Error;
				log.Warn("Invalid caster {0} for spell {1} in EffectHandler: {2}", Effect.Spell, m_cast.CasterObject, this);
			}
		}

		/// <summary>
		/// Used for one-shot damage and healing effects
		/// </summary>
		public int CalcDamageValue()
		{
			var val = CalcEffectValue();
			if (CurrentTargetNo > 0)
			{
				// chain target damage comes with diminishing returns
				return Effect.GetMultipliedValue(m_cast.CasterChar, val, CurrentTargetNo);
			}
			return val;
		}

		/// <summary>
		/// Used for one-shot damage and healing effects
		/// </summary>
		public int CalcDamageValue(int targetNo)
		{
			var val = CalcEffectValue();
			if (targetNo > 0)
			{
				// chain target damage comes with diminishing returns
				return Effect.GetMultipliedValue(m_cast.CasterChar, val, targetNo);
			}
			return val;
		}

		public int CalcEffectValue()
		{
			return Effect.CalcEffectValue(m_cast.CasterReference);
		}

		public float GetRadius()
		{
			return Effect.GetRadius(m_cast.CasterReference);
		}
		#endregion

		#region Debugging / Testing
		public void SendEffectInfoToCaster(string extra)
		{
			if (m_cast.CasterChar != null)
			{
				m_cast.CasterChar.SendSystemMessage("SpellEffect {0} {1}", GetType(), extra);
			}
		}
		#endregion

		public override string ToString()
		{
			return GetType().Name + " - Spell: " + Effect.Spell.FullName + (m_cast != null ? (", Caster: " + m_cast.CasterObject) : "");
		}
	}
}