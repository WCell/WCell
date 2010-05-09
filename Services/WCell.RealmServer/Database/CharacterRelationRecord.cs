/*************************************************************************
 *
 *   file		: CharacterRelationRecord.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-25 18:19:39 +0100 (ma, 25 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1222 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using Castle.ActiveRecord;
using WCell.RealmServer.Interaction;
using WCell.Core.Database;

namespace WCell.RealmServer.Database
{
	/// <summary>
	/// Represents a character relationship entry in the database
	/// </summary>
	[ActiveRecord(Access = PropertyAccess.Property)]
	public class CharacterRelationRecord : WCellRecord<CharacterRelationRecord>
	{
		private static readonly NHIdGenerator _idGenerator =
			new NHIdGenerator(typeof(SpellRecord), "RecordId");

		/// <summary>
		/// Returns the next unique Id for a new SpellRecord
		/// </summary>
		public static long NextId()
		{
			return _idGenerator.Next();
		}

		[Field("CharacterId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private long _characterId;

		[Field("RelatedCharacterId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private long _relatedCharacterId;

		public CharacterRelationRecord()
		{
		}

		public CharacterRelationRecord(uint charId, uint relatedCharId, CharacterRelationType type)
		{
			CharacterId = charId;
			RelatedCharacterId = relatedCharId;
			RelationType = type;
			New = true;
			CharacterRelationGuid = NextId();
		}

		[PrimaryKey(PrimaryKeyType.Assigned)]
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

		[Property(NotNull = true)]
		public CharacterRelationType RelationType
		{
			get;
			set;
		}

		[Property]
		public string Note
		{
			get;
			set;
		}
	}
}
