/*************************************************************************
 *
 *   file		: Prospecting.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-02-04 01:38:40 +0800 (Mon, 04 Feb 2008) $
 *   last author	: $LastChangedBy: domiii $
 *   revision		: $Rev: 97 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants.Items;
using WCell.Constants.Looting;
using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells.Effects
{
	public class MillingEffectHandler : ItemConvertEffectHandler
	{
		public MillingEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override void Initialize(ref SpellFailedReason failReason)
		{
			if (!m_cast.UsedItem.Template.Flags.HasFlag(ItemFlags.Millable))
			{
				failReason = SpellFailedReason.CantBeMilled;
			}
			else
			{
				base.Initialize(ref failReason);
			}
		}

		public override LootEntryType LootEntryType
		{
			get { return LootEntryType.Milling; }
		}
	}
}