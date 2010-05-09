/*************************************************************************
 *
 *   file		: ModLanguage.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-12-20 21:43:00 +0100 (sø, 20 dec 2009) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1148 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants.Misc;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
	/// <summary>
	/// Temporarily changes the language of the holder
	/// </summary>
	public class ModLanguageHandler : AuraEffectHandler
	{
		protected internal override void Apply()
		{
			if (m_aura.Auras.Owner is Character)
			{
				ChatLanguage lang = (ChatLanguage)m_spellEffect.MiscValue;
				m_aura.Auras.Owner.SpokenLanguage = lang;
			}
		}

		protected internal override void Remove(bool cancelled)
		{
			if (m_aura.Auras.Owner is Character)
			{
				m_aura.Auras.Owner.SpokenLanguage = ChatLanguage.Universal;
			}
		}
	}
};
