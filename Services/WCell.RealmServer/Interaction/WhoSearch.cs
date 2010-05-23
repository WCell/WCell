/*************************************************************************
 *
 *   file		: WhoSearch.cs
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

using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.World;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Interaction
{
    /// <summary>
    /// <see cref="CharacterSearch"/> derived class customized to perform the Who List search 
    /// </summary>
    public sealed class WhoSearch : CharacterSearch
    {
        #region Public Properties
		/// <summary>
		/// Guild name search filter. If not set this filter is ignored when performing the search.
		/// </summary>
		public string GuildName
		{
			get;
			set;
		}

		/// <summary>
		/// Zones search filter. If not set this filter is ignored when performing the search.
		/// </summary>
		public List<ZoneId> Zones
		{
			get;
			set;
		}

        /// <summary>
        /// Names search filter. If not set this filter is ignored when performing the search.
        /// </summary>
		public List<string> Names
		{
			get;
			set;
		}

        /// <summary>
        /// Faction search filter. If not set this filter is ignored when performing the search.
        /// </summary>
		public FactionGroup Faction
		{
			get;
			set;
		}

		/// <summary>
		/// Race search filter. If not set this filter is ignored when performing the search.
		/// </summary>
		public RaceMask2 RaceMask
		{
			get;
			set;
		}

		/// <summary>
		/// Class search filter. If not set this filter is ignored when performing the search.
		/// </summary>
		public ClassMask2 ClassMask
		{
			get;
			set;
		}
        #endregion

		public WhoSearch()
		{
			GuildName = string.Empty;
			Zones = new List<ZoneId>();
			Names = new List<string>();
			Faction = FactionGroup.Invalid;
			RaceMask = RaceMask2.All;
			ClassMask = ClassMask2.All;
		}

        /// <summary>
        /// Method used to add custom search criterias. Added Who List custom search criterias to the default ones.
        /// </summary>
        /// <param name="character">The <see cref="Character"/> to be checked against custom search criterias.</param>
        /// <returns>True if the character pass all custom search criterias. False otherwise.</returns>
        protected override bool IncludeCharacter(Character character)
        {
            if (!base.IncludeCharacter(character))
                return false;

			if (Faction != FactionGroup.Invalid && character.Faction.Group != Faction)
				return false;

			if (!string.IsNullOrEmpty(GuildName) /*&& character.Guild.Name != GuildName */)
				return false;

			if (!RaceMask.HasFlag(character.RaceMask2))
				return false;

			if (!ClassMask .HasFlag(character.ClassMask2))
				return false;

			if (Zones.Count > 0 && !Zones.Contains(character.Zone.Id))
                return false;

            if (Names.Count > 0 && !Names.Contains(character.Name.ToLower()))
                return false;

            return true;
        }
    }
}
