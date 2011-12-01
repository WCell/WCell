/*************************************************************************
 *
 *   file		: CharacterSearch.cs
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

using System;
using System.Collections.Generic;
using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Interaction
{
    /// <summary>
    /// Base class used for <see cref="Character"/> searches.
    /// <remarks>
    /// This class provides common character searching criterias, if you need to include your own custom criterias
    /// just derive from this class and override the <see cref="IncludeCharacter"/> method.
    /// By default this class compares the online characters matching the search criterias. If you need to conduct 
    /// searches on offline or custom Characters, you can do so overriding the <see cref="GetCharacters"/> method.
    /// </remarks>
    /// </summary>
    public class CharacterSearch
    {
        private string m_name;
        private EntityId m_id = EntityId.Zero;
        private byte m_minLevel = byte.MinValue;
        private byte m_maxLevel = byte.MaxValue;
        private uint m_maxResultCount;

        #region Public Properties
        /// <summary>
		/// Character name search filter. If not set this filter is ignored when performing the search.
        /// </summary>
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        /// <summary>
		/// Character <see cref="EntityId"/> search filter. If not set this filter is ignored when performing the search.
        /// </summary>
        public EntityId EntityId
        {
            get { return m_id; }
            set { m_id = value; }
        }

        /// <summary>
		/// Character min level search filter. If not set this filter is ignored when performing the search.
        /// </summary>
        public byte MinLevel
        {
            get { return m_minLevel; }
            set { m_minLevel = value; }
        }

        /// <summary>
		/// Character max level search filter. If not set this filter is ignored when performing the search.
        /// </summary>
        public byte MaxLevel
        {
            get { return m_maxLevel; }
            set { m_maxLevel = value; }
        }
        
        /// <summary>
        /// The maximum ammount of Characters matching the search criterias to return
        /// </summary>
        public uint MaxResultCount
        {
            get { return m_maxResultCount; }
            set { m_maxResultCount = value; }
        }
        #endregion

        /// <summary>
        /// Retrieves a list of the matched characters based on the search criterias.
        /// </summary>
        /// <returns>A list of the matched characters</returns>
        public ICollection<Character> RetrieveMatchedCharacters()
        { 
            var matchedCharacters = new List<Character>();
            var characters = GetCharacters();

            uint totalMatches = 0;
            foreach (var character in characters)
            {
                if (!IncludeCharacter(character)) 
					continue;

                totalMatches++;

                //If all checks passed the player matched all criterias, so we add it to the matched list
                //If we reached the result count then we stop adding matches
				if (totalMatches <= m_maxResultCount)
					matchedCharacters.Add(character);
				else
					break;
            }
            return matchedCharacters;
        }

        /// <summary>
        /// Used to retrieve the character list used in the search.
        /// By default it retrieves the online characters of the <see cref="World"/>.
        /// Override if you need to search offline characters or custom character lists.
        /// </summary>
        /// <returns>The character list to be searched</returns>
        protected virtual ICollection<Character> GetCharacters()
        {
			return World.GetAllCharacters();
        }

        /// <summary>
        /// Used by inheriters to allow custom search criterias to be performed.
        /// </summary>
        /// <param name="character">The <see cref="Character"/> to be checked against custom search criterias.</param>
        /// <returns>True if the character pass all custom search criterias. False otherwise.</returns>
        protected virtual bool IncludeCharacter(Character character)
        {
            if (m_id != EntityId.Zero && character.EntityId != m_id)
                return false;

            if (m_name.Length > 0 && character.Name.IndexOf(m_name, StringComparison.InvariantCultureIgnoreCase) < 0)
                return false;

            if (character.Level < m_minLevel || character.Level > m_maxLevel)
                return false;

            return true;
        }
    }
}