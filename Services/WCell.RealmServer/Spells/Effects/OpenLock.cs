/*************************************************************************
 *
 *   file		: OpenLock.cs
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

using WCell.Util.Logging;
using WCell.Constants;
using WCell.Constants.GameObjects;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Skills;
using WCell.Util;
using WCell.Constants.Updates;
using System;

namespace WCell.RealmServer.Spells.Effects
{
	/// <summary>
	/// Tries to open a GameObject or Item or disarm a trap
	/// </summary>
	public class OpenLockEffectHandler : SpellEffectHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		ILockable lockable;
		LockOpeningMethod method;
		Skill skill;

		public OpenLockEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override void Initialize(ref SpellFailedReason failReason)
		{
			if (m_cast.Selected != null)
			{
				lockable = m_cast.Selected as GameObject;
			}
			else
			{
				lockable = m_cast.TargetItem;
			}

			if (lockable == null)
			{
				failReason = SpellFailedReason.BadTargets;
			}
			else
			{
				var lck = lockable.Lock;
				var chr = m_cast.CasterChar;

				if (lck == null)
				{
					log.Warn("Using OpenLock on object without Lock: " + lockable);
					failReason = SpellFailedReason.Error;
					return;
				}
				if (chr == null)
				{
					log.Warn("Using OpenLock without Character: " + chr);
					failReason = SpellFailedReason.Error;
					return;
				}

				if (!lck.IsUnlocked)
				{
					var type = (LockInteractionType)Effect.MiscValue;
					if (lck.Keys.Length > 0 && m_cast.CasterItem != null)
					{
						if (!lck.Keys.Contains(key => key.KeyId == m_cast.CasterItem.Template.ItemId))
						{
							failReason = SpellFailedReason.ItemNotFound;
							return;
						}
					}
					else if (!lck.Supports(type))
					{
						failReason = SpellFailedReason.BadTargets;
						return;
					}

					if (type != LockInteractionType.None)
					{
						foreach (var openingMethod in lck.OpeningMethods)
						{
							if (openingMethod.InteractionType == type)
							{
								if (openingMethod.RequiredSkill != SkillId.None)
								{
									skill = chr.Skills[openingMethod.RequiredSkill];
									if (skill == null || skill.ActualValue < openingMethod.RequiredSkillValue)
									{
										failReason = SpellFailedReason.MinSkill;
									}
								}
								method = openingMethod;
								break;
							}
						}

						if (method == null)
						{
							// we are using the wrong kind of spell on the target
							failReason = SpellFailedReason.BadTargets;
						}
					}
				}

				if (failReason != SpellFailedReason.Ok)
				{
					// spell failed
					if (lockable is GameObject && ((GameObject)lockable).Entry.IsConsumable)
					{
						// re-enable GO
						((GameObject)lockable).State = GameObjectState.Enabled;
					}
				}
			}
		}

		public override void Apply()
		{
			if (skill != null)
			{
				// check if the skill works
				var reqSkill = method.RequiredSkillValue;
				var diff = skill.ActualValue - (reqSkill == 1 ? 0 : reqSkill);

				if (!CheckSuccess(diff))
				{
					// failed
					m_cast.Cancel(SpellFailedReason.TryAgain);
					return;
				}

				// skill gain
				var skillVal = skill.ActualValue;
				skillVal += (ushort)Gain(diff);
				skillVal = Math.Min(skillVal, skill.MaxValue);
				skill.CurrentValue = (ushort)skillVal;
			}

			// open lock
			var chr = m_cast.CasterChar;
			chr.AddMessage(() =>
			{
				if (lockable is ObjectBase && !((ObjectBase)lockable).IsInWorld) return;

				LockEntry.Handle(chr, lockable, method != null ? method.InteractionType : LockInteractionType.None);
			});
		}

		protected override void Apply(WorldObject target)
		{
			// this is not a multiple target effect
		}

		public override ObjectTypes CasterType
		{
			get { return ObjectTypes.Player; }
		}

		public override bool HasOwnTargets
		{
			get { return false; }
		}


		public int Gain(uint diff)
		{
			int chance;
			if (diff >= SkillAbility.GreyDiff)
			{
				return 0;
			}
			else if (diff >= SkillAbility.GreenDiff)
			{
				chance = SkillAbility.GainChanceGreen;
			}
			else if (diff >= SkillAbility.YellowDiff)
			{
				chance = SkillAbility.GainChanceYellow;
			}
			else
			{
				chance = SkillAbility.GainChanceOrange;
			}

			return (Utility.Random() % 1000) < chance ? SkillAbility.GainAmount : 0;
		}

		public bool CheckSuccess(uint diff)
		{
			int chance;
			if (diff >= SkillAbility.GreyDiff)
			{
				chance = SkillAbility.SuccessChanceGrey;
			}
			else if (diff >= SkillAbility.GreenDiff)
			{
				chance = SkillAbility.SuccessChanceGreen;
			}
			else if (diff >= SkillAbility.YellowDiff)
			{
				chance = SkillAbility.SuccessChanceYellow;
			}
			else if (diff >= 0)
			{
				chance = SkillAbility.SuccessChanceOrange;
			}
			else
			{
				return false;
			}

			return (Utility.Random() % 1000) < chance;
		}
	}
}