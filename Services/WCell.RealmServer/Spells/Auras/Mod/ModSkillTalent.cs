/*************************************************************************
 *
 *   file		: ModSkillTalent.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-03-07 07:58:12 +0100 (lø, 07 mar 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 784 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants.Skills;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Skills;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
	/// <summary>
	/// Adds a flat modifier to one Skill
	/// </summary>
	public class ModSkillTalentHandler : AuraEffectHandler
	{
		Skill skill;
		
		protected internal override void Apply()
		{
			if (m_aura.Auras.Owner is Character) {
				skill = ((Character)m_aura.Auras.Owner).Skills[(SkillId)m_spellEffect.MiscValue];
				if (skill != null) {
					skill.Modifier += (short)EffectValue;
				}
			}
		}

		protected internal override void Remove(bool cancelled)
		{
			if (skill != null) {
				skill.Modifier -= (short)EffectValue;
			}
		}
	}
};