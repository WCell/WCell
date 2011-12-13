/*************************************************************************
 *
 *   file		: ApplyAura.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-28 13:29:18 +0100 (to, 28 jan 2010) $

 *   revision		: $Rev: 1230 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.Collections.Generic;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
	/// <summary>
	/// Applies a single AuraEffectHandler to every target
	/// </summary>
	public class ApplyAuraEffectHandler : SpellEffectHandler
	{
		List<SingleAuraApplicationInfo> m_auraEffectHandlers;

		public ApplyAuraEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		/// <summary>
		/// Aura-Spells should always have targets
		/// </summary>
		public override bool HasOwnTargets
		{
			get { return true; }
		}

		public override SpellFailedReason Initialize()
		{
			m_auraEffectHandlers = new List<SingleAuraApplicationInfo>(3);
			return SpellFailedReason.Ok;
		}

		public override SpellFailedReason InitializeTarget(WorldObject target)
		{
			var failedReason = SpellFailedReason.Ok;
			var effectHandler = Effect.CreateAuraEffectHandler(m_cast.CasterReference, (Unit)target, ref failedReason, m_cast);

			if (failedReason == SpellFailedReason.Ok)
			{
				m_auraEffectHandlers.Add(new SingleAuraApplicationInfo((Unit)target, effectHandler));
			}
			return failedReason;
		}

		public override void Apply()
		{
			// do nothing
		}

		public override ObjectTypes TargetType
		{
			get { return ObjectTypes.Unit; }
		}

		/// <summary>
		/// Adds all AuraEffectHandlers that this SpellEffectHandler created, 
		/// indexed by the target they have been created for (if this effect creates Auras at all)
		/// </summary>
		public void AddAuraHandlers(List<AuraApplicationInfo> applicationInfos)
		{
			foreach (var auraHandler in m_auraEffectHandlers)
			{
				foreach (var info in applicationInfos)
				{
					if (info.Target == auraHandler.Target && info.Handlers != null)
					{
						info.Handlers.Add(auraHandler.Handler);
						break;
					}
				}

				//if (!found)
				//{
				//    target not applicable
				//    applicationInfos.Add(new AuraApplicationInfo(auraHandler.Target, auraHandler.Handler));
				//}
			}
		}
	}
}