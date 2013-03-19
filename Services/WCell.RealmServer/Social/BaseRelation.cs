/*************************************************************************
 *
 *   file		: BaseRelation.cs
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
using WCell.RealmServer.Database;
using WCell.RealmServer.Database.Entities;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Interaction
{
    /// <summary>
    /// Represents a relationship between two <see cref="Character"/> entities.
    /// </summary>
    public abstract class BaseRelation : IBaseRelation
	{
		public static readonly IBaseRelation[] EmptyRelations = new IBaseRelation[0];
		public static readonly HashSet<IBaseRelation> EmptyRelationSet = new HashSet<IBaseRelation>();

		#region Properties
		/// <summary>
		/// The Character who created this Relation
		/// </summary>
        public virtual uint CharacterId
        {
            get;
            set;
		}

		/// <summary>
		/// The related Character with who this Relation is with
		/// </summary>
		public virtual uint RelatedCharacterId
        {
            get;
            set;
        }

        /// <summary>
        /// The relation type
        /// </summary>
        public abstract CharacterRelationType Type
        {
            get;
        }

		/// <summary>
		/// A note describing the relation
		/// </summary>
		public virtual string Note
		{
			get;
			set;
		}

        /// <summary>
        /// Indicates if the relation requires sending a notification when a player change 
        /// its online status
        /// </summary>
        public virtual bool RequiresOnlineNotification
        {
            get { return false; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        protected BaseRelation()
        {
        }

        /// <summary>
        /// Creates a new character relation based on the chars EntityId
        /// </summary>
        protected BaseRelation(uint charId, uint relatedCharId)
        {
            CharacterId = charId;
            RelatedCharacterId = relatedCharId;
        }
        #endregion

		public override bool Equals(object otherRelation)
        {
            if (!(otherRelation is BaseRelation))
                return false;

			var other = otherRelation as BaseRelation;
            return CharacterId == other.CharacterId && 
                RelatedCharacterId == other.RelatedCharacterId && 
                Type == other.Type;
        }

		public override int GetHashCode()
		{
			// persistant and unique if only used within the context of one Character
			return (int)RelatedCharacterId;
		}

        public virtual bool Validate(CharacterRecord charInfo, CharacterRecord relatedCharInfo, 
            out RelationResult relResult)
        {
            relResult = RelationResult.FRIEND_DB_ERROR;
            return true;
        }
    }
}