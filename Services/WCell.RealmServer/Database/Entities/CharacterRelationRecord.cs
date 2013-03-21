/*************************************************************************
 *
 *   file		: CharacterRelationRecord.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-25 18:19:39 +0100 (ma, 25 jan 2010) $

 *   revision		: $Rev: 1222 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.RealmServer.Interaction;

namespace WCell.RealmServer.Database.Entities
{
	/// <summary>
	/// Represents a character relationship entry in the database
	/// </summary>
	public class CharacterRelationRecord
	{
		private long _characterId;
		private long _relatedCharacterId;

		public CharacterRelationRecord()
		{
		}

		public CharacterRelationRecord(uint charId, uint relatedCharId, CharacterRelationType type)
		{
			CharacterId = charId;
			RelatedCharacterId = relatedCharId;
			RelationType = type;
			CharacterRelationGuid = 0;//TODO: Work out what best to do here
		}

		public long CharacterRelationGuid
		{
			get;
			set;
		}

		public uint CharacterId
		{
			get { return (uint)_characterId; }
			set { _characterId = value; }
		}

		public uint RelatedCharacterId
		{
			get { return (uint)_relatedCharacterId; }
			set { _relatedCharacterId = value; }
		}

		public CharacterRelationType RelationType
		{
			get;
			set;
		}

		public string Note
		{
			get;
			set;
		}
	}
}