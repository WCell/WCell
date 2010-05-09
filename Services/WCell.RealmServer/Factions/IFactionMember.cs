/*************************************************************************
 *
 *   file		: IFactionMember.cs
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

using WCell.Constants.Factions;

namespace WCell.RealmServer.Factions
{
	/// <summary>
	/// An interface for any WorldObject that can have a Faction
	/// </summary>
	public interface IFactionMember
	{
		Faction Faction
		{
			get;
			set;
		}

		FactionId FactionId
		{
			get;
			set;
		}

		/// <summary>
		/// Indicates whether the 2 objects have a good relationship
		/// </summary>
		bool IsFriendlyWith(IFactionMember opponent);

		/// <summary>
		/// Indicates whether this Object is in party or otherwise allied with the given obj
		/// </summary>
		bool IsAlliedWith(IFactionMember obj);
	}
}
