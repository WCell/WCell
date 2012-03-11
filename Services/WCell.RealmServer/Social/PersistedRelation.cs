/*************************************************************************
 *
 *   file		: PersistedRelation.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-24 02:53:28 +0100 (sø, 24 jan 2010) $

 *   revision		: $Rev: 1215 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.Linq;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Interaction
{
    /// <summary>
    /// Represents a relationship between two <see cref="Character"/> entities, persisted in the db.
    /// </summary>
    public abstract class PersistedRelation : BaseRelation
    {
        private readonly CharacterRelationRecord m_charRelationRecord;

        #region Properties

        public override uint CharacterId
        {
            get
            {
                return m_charRelationRecord.CharacterId;
            }
            set
            {
                m_charRelationRecord.CharacterId = value;
            }
        }

        public override uint RelatedCharacterId
        {
            get
            {
                return m_charRelationRecord.RelatedCharacterId;
            }
            set
            {
                m_charRelationRecord.RelatedCharacterId = value;
            }
        }

        public override string Note
        {
            get
            {
                return m_charRelationRecord.Note;
            }
            set
            {
                m_charRelationRecord.Note = value;
            }
        }

        #endregion Properties

        /// <summary>
        /// Default constructor
        /// </summary>
        public PersistedRelation()
        {
            m_charRelationRecord = new CharacterRelationRecord();
        }

        /// <summary>
        /// Creates a new character relation based on the chars EntityId
        /// </summary>
        public PersistedRelation(uint charId, uint relatedCharId)
        {
            m_charRelationRecord = new CharacterRelationRecord(charId, relatedCharId, this.Type);
        }

        /// <summary>
        /// Creates a new character relation based on a <see cref="CharacterRelationRecord"/>
        /// </summary>
        protected PersistedRelation(CharacterRelationRecord relation)
        {
            m_charRelationRecord = relation;
        }

        /// <summary>
        /// Saves this instance to the DB
        /// </summary>
        public virtual void SaveToDB()
        {
            m_charRelationRecord.Save();
        }

        /// <summary>
        /// Delete this instance from the database
        /// </summary>
        public virtual void Delete()
        {
            m_charRelationRecord.Delete();
        }

        #region Static Methods

        /// <summary>
        /// Retrieves the list of character relations
        /// </summary>
        /// <returns>The list of all characters relations.</returns>
        public static BaseRelation[] GetAll()
        {
            CharacterRelationRecord[] relations = CharacterRelationRecord.FindAll();

            return relations.Select(crr => RelationMgr.CreateRelation(crr)).ToArray();
        }

        /// <summary>
        /// Retrieves the list of character relations of a character
        /// </summary>
        /// <param name="charLowId">The character Id</param>
        /// <returns>The list of relations of the character.</returns>
        public static BaseRelation[] GetByCharacterId(uint charLowId)
        {
            CharacterRelationRecord[] relations =
                CharacterRelationRecord.FindAllByProperty("_characterId", (long)charLowId);

            return relations.Select(crr => RelationMgr.CreateRelation(crr)).ToArray();
        }

        #endregion Static Methods
    }
}