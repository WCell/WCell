/*************************************************************************
 *
 *   file		: SpellEnums.cs
 *   copyright		: (C) The WCell Team
 *   email			: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-05-11 22:39:47 +0800 (Sun, 11 May 2008) $
 *   last author	: $LastChangedBy: domiii $
 *   revision		: $Rev: 335 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.Relations
{

	[Flags]
	public enum RelationTypeFlag : uint
	{
		None = 0x00,
		Friend = 0x01,
		Ignore = 0x02,
		Muted = 0x04,
		RecruitAFriend = 0x08,
	}

	[Flags]
	public enum LFGRolesMask : byte
	{
		None = 0,
		Leader = 1,
		Tank = 2,
		Healer = 4,
		Damage = 8
	}
}