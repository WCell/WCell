/*************************************************************************
 *
 *   file		: RelationMgr.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WCell.Constants;
using WCell.Constants.Relations;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.RealmServer.Database;
using WCell.RealmServer.Database.Entities;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Res;
using WCell.Util.Logging;

namespace WCell.RealmServer.Interaction
{
	/// <summary>
	/// TODO: Clean up
	/// TODO: Get rid of events and call methods directly to ensure this to be deterministic
	/// 
	/// Defines all kinds of Relations between Characters, mainly Friends, Ignores, Mutings, Invitations to Groups and Guilds etc.
	/// Characters can have active Relations (which were triggered by the Character himself) and 
	/// passive Relations (which are Relations that others established with the corresponding Character).
	/// </summary>
	public enum CharacterRelationType
	{
		Invalid = 0,
		Friend = 1,
		Ignored = 2,
		Muted = 3,
		GroupInvite = 4,
		GuildInvite = 5,
		Count
	}

	public sealed class RelationMgr : Manager<RelationMgr>
	{
		private readonly Dictionary<uint, HashSet<IBaseRelation>>[] m_activeRelations;
		private readonly Dictionary<uint, HashSet<IBaseRelation>>[] m_passiveRelations;
		private readonly ReaderWriterLockSlim m_lock;
		private static readonly Logger _log = LogManager.GetCurrentClassLogger();

		private RelationMgr()
		{
			m_activeRelations = new Dictionary<uint, HashSet<IBaseRelation>>[(int)CharacterRelationType.Count];
			m_passiveRelations = new Dictionary<uint, HashSet<IBaseRelation>>[(int)CharacterRelationType.Count];

			for (int i = 1; i < (int)CharacterRelationType.Count; i++)
			{
				m_activeRelations[i] = new Dictionary<uint, HashSet<IBaseRelation>>();
				m_passiveRelations[i] = new Dictionary<uint, HashSet<IBaseRelation>>();
			}

			m_lock = new ReaderWriterLockSlim();
		}

		private void Initialize()
		{
			BaseRelation[] found;
			try
			{
				found = PersistedRelation.GetAll();
			}
			catch (Exception e)
			{
				RealmWorldDBMgr.OnDBError(e);
				found = PersistedRelation.GetAll();
			}

			m_lock.EnterWriteLock();
			try
			{
				foreach (PersistedRelation relation in found)
				{
					HashSet<IBaseRelation> relations;
					if (!m_activeRelations[(int)relation.Type].TryGetValue(relation.CharacterId, out relations))
					{
						m_activeRelations[(int)relation.Type].Add(relation.CharacterId, relations = new HashSet<IBaseRelation>());
					}
					relations.Add(relation);

					if (!m_passiveRelations[(int)relation.Type].TryGetValue(relation.RelatedCharacterId, out relations))
					{
						m_passiveRelations[(int)relation.Type].Add(relation.RelatedCharacterId, relations = new HashSet<IBaseRelation>());
					}
					relations.Add(relation);
				}
			}
			finally
			{
				m_lock.ExitWriteLock();
			}
		}

		/// <summary>
		/// The lock to synchronize with, when iterating over this Manager's Relations etc.
		/// </summary>
		public ReaderWriterLockSlim Lock
		{
			get
			{
				return m_lock;
			}
		}

		private void LoadRelations(uint charId)
		{
			m_lock.EnterWriteLock();
			try
			{
				//Add the character relations to the global dictionaries
				foreach (PersistedRelation cr in PersistedRelation.GetByCharacterId(charId))
				{
					HashSet<IBaseRelation> relations;
					if (!m_activeRelations[(int)cr.Type].TryGetValue(cr.CharacterId, out relations))
					{
						m_activeRelations[(int)cr.Type].Add(cr.CharacterId, relations = new HashSet<IBaseRelation>());
					}
					relations.Add(cr);

					if (!m_passiveRelations[(int)cr.Type].TryGetValue(cr.CharacterId, out relations))
					{
						m_passiveRelations[(int)cr.Type].Add(cr.RelatedCharacterId, relations = new HashSet<IBaseRelation>());
					}
					relations.Add(cr);
				}
			}
			finally
			{
				m_lock.ExitWriteLock();
			}
		}

		public static bool IsIgnoring(uint chr, uint ignoredCharId)
		{
			return Instance.GetRelation(chr, ignoredCharId, CharacterRelationType.Ignored) != null;
		}

		/// <summary>
		/// Only needed if Character gets removed?
		/// </summary>
		public void RemoveRelations(uint lowId)
		{
			m_lock.EnterWriteLock();
			try
			{
				for (int i = 1; i < (int)CharacterRelationType.Count; i++)
				{
					HashSet<IBaseRelation> relations;
					if (m_activeRelations[i].TryGetValue(lowId, out relations))
					{
						foreach (var relation in relations)
						{
							if (relation is PersistedRelation)
							{
								((PersistedRelation)relation).Delete();
							}
						}
					}

					if (m_passiveRelations[i].TryGetValue(lowId, out relations))
					{
						foreach (var relation in relations)
						{
							if (relation is PersistedRelation)
							{
								((PersistedRelation)relation).Delete();
							}
						}
					}

					m_activeRelations[i].Remove(lowId);
					m_passiveRelations[i].Remove(lowId);
				}
			}
			finally
			{
				m_lock.ExitWriteLock();
			}
		}

		/// <summary>
		/// Retrieves all Relations that the given Character started with others
		/// </summary>
		/// <param name="charLowId">The <see cref="EntityId"/> of the character wich relations are requested</param>
		/// <param name="relationType">The type of the relation</param>
		/// <returns>The list of the related characters relations.</returns>
		public HashSet<IBaseRelation> GetRelations(uint charLowId, CharacterRelationType relationType)
		{
			if (charLowId == 0 || relationType == CharacterRelationType.Invalid)
				return BaseRelation.EmptyRelationSet;

		    m_lock.EnterReadLock();
			try
			{
			    HashSet<IBaseRelation> relations;
			    if (m_activeRelations[(int)relationType].TryGetValue(charLowId, out relations))
				{
					return relations;
				}
			}
			finally
			{
				m_lock.ExitReadLock();
			}
			return BaseRelation.EmptyRelationSet;
		}

		/// <summary>
		/// Retrieves all Relations of the given type that others have started with the given Character
		/// </summary>
		/// <param name="charLowId">The <see cref="EntityId"/> of the character wich relations are requested</param>
		/// <param name="relationType">The type of the relation</param>
		public HashSet<IBaseRelation> GetPassiveRelations(uint charLowId, CharacterRelationType relationType)
		{
			if (charLowId == 0 || relationType == CharacterRelationType.Invalid)
				return BaseRelation.EmptyRelationSet;

		    m_lock.EnterReadLock();
			try
			{
			    HashSet<IBaseRelation> relations;
			    if (m_passiveRelations[(int)relationType].TryGetValue(charLowId, out relations))
				{
					return relations;
				}
			}
			finally
			{
				m_lock.ExitReadLock();
			}
			return BaseRelation.EmptyRelationSet;
		}

		/// <summary>
		/// Returns whether the Character with the given low Entityid has any passive relations of the given type
		/// </summary>
		/// <param name="charLowId"></param>
		/// <param name="relationType"></param>
		/// <returns></returns>
		public bool HasPassiveRelations(uint charLowId, CharacterRelationType relationType)
		{
			if (charLowId == 0 || relationType == CharacterRelationType.Invalid)
				return false;

			HashSet<IBaseRelation> relations;
			m_lock.EnterReadLock();

			try
			{
				if (m_passiveRelations[(int)relationType].TryGetValue(charLowId, out relations))
				{
					return (relations.Count > 0);
				}
				else
				{
					return false;
				}
			}
			finally
			{
				m_lock.ExitReadLock();
			}
		}

		/// <summary>
		/// Retrieves a current relationship between the given characters and the type
		/// </summary>
		/// <param name="charId">The first character EntityId</param>
		/// <param name="relatedCharId">The related character low EntityId</param>
		/// <param name="relationType">The relationship type</param>
		/// <returns>A <see cref="BaseRelation"/> object representing the relation;
		/// null if the relation wasnt found.</returns>
		public BaseRelation GetRelation(uint charId, uint relatedCharId, CharacterRelationType relationType)
		{
			if (charId == 0 || relatedCharId == 0 || relationType == CharacterRelationType.Invalid)
				return null;

		    m_lock.EnterReadLock();
			try
			{
			    HashSet<IBaseRelation> relations;
			    if (m_activeRelations[(int)relationType].TryGetValue(charId, out relations))
				{
					foreach (BaseRelation cr in relations)
					{
						if (cr.RelatedCharacterId == relatedCharId)
							return cr;
					}
				}
			}
			finally
			{
				m_lock.ExitReadLock();
			}
			return null;
		}

		/// <summary>
		/// Sets a relation note text
		/// </summary>
		/// <param name="charId">The first character EntityId</param>
		/// <param name="relatedCharId">The related character low EntityId</param>
		/// <param name="note">The note to be assigned to the relation</param>
		/// <param name="relationType">The relationship type</param>
		public void SetRelationNote(uint charId, uint relatedCharId, string note, CharacterRelationType relationType)
		{
			if (charId == 0 || relatedCharId == 0 || string.IsNullOrEmpty(note) || relationType == CharacterRelationType.Invalid)
				return;

			BaseRelation relation = GetRelation(charId, relatedCharId, relationType);

			if (relation != null)
			{
				relation.Note = note;

				if (relation is PersistedRelation)
				{
					((PersistedRelation)relation).SaveToDB();
				}
			}
		}

		/// <summary>
		/// Returns whether the given char has the given relationType with the given relatedChar
		/// </summary>
		/// <param name="charId">The first character EntityId</param>
		/// <param name="relatedCharId">The related character EntityId</param>
		/// <param name="relationType">The relationship type</param>
		/// <returns>True if the relation exist. False otherwise</returns>
		public bool HasRelation(uint charId, uint relatedCharId, CharacterRelationType relationType)
		{
			return GetRelation(charId, relatedCharId, relationType) != null;
		}

		/// <summary>
		/// Adds a character relation
		/// </summary>
		/// <param name="character">The first character in the relation</param>
		/// <param name="relatedCharName">The related character name</param>
		/// <param name="note">A note describing the relation. Used for Friend only relation types</param>
		/// <param name="relationType">The relation type</param>
		internal void AddRelation(Character character, string relatedCharName, string note,
			CharacterRelationType relationType)
		{
		    var target = World.GetCharacter(relatedCharName, false);
			CharacterRecord relatedCharInfo;
			if (target != null)
			{
				relatedCharInfo = target.Record;
			}
			else
			{
				relatedCharInfo = CharacterRecord.GetRecordByName(relatedCharName);
			}

			if (relatedCharInfo != null)
			{
				BaseRelation relation = CreateRelation(character.EntityId.Low, relatedCharInfo.EntityLowId, relationType);
				relation.Note = note;

			    RelationResult relResult;
			    if (!relation.Validate(character.Record, relatedCharInfo, out relResult))
				{
					_log.Debug(Resources.CharacterRelationValidationFailed, character.Name,
						character.EntityId, relatedCharName, relatedCharInfo.EntityLowId, relationType, relResult);
				}
				else
				{
					AddRelation(relation);
				}

				//Send relation status to the client
				if (relResult == RelationResult.FRIEND_ADDED_ONLINE)
				{
					SendFriendOnline(character, target, note, true);
				}
				else
				{
					SendFriendStatus(character, relatedCharInfo.EntityLowId, note, relResult);
				}
			}
			else
			{
				//Send relation status to the client
				SendFriendStatus(character, 0, note, RelationResult.FRIEND_NOT_FOUND);
			}
		}

		/// <summary>
		/// Adds a character relation
		/// </summary>
		/// <param name="relation">The relation to be added</param>
		public void AddRelation(BaseRelation relation)
		{
			try
			{
				//Persists the relation to the db if persistable
				if (relation is PersistedRelation)
				{
					(relation as PersistedRelation).SaveToDB();
				}

				HashSet<IBaseRelation> relations, relatedRelations;

				m_lock.EnterWriteLock();
				try
				{
					if (!m_activeRelations[(int)relation.Type].TryGetValue(relation.CharacterId, out relations))
					{
						m_activeRelations[(int)relation.Type].Add(relation.CharacterId, relations = new HashSet<IBaseRelation>());
					}

					if (!m_passiveRelations[(int)relation.Type].TryGetValue(relation.RelatedCharacterId, out relatedRelations))
					{
						m_passiveRelations[(int)relation.Type].Add(relation.RelatedCharacterId, relatedRelations = new HashSet<IBaseRelation>());
					}

					relations.Add(relation);
					relatedRelations.Add(relation);
				}
				finally
				{
					m_lock.ExitWriteLock();
				}

				_log.Debug(Resources.CharacterRelationAdded, string.Empty, relation.CharacterId,
				string.Empty, relation.RelatedCharacterId, relation.Type, 0);
			}
			catch (Exception ex)
			{
				LogUtil.ErrorException(ex, string.Format(Resources.CharacterRelationAddedFailed, string.Empty, relation.CharacterId,
				string.Empty, relation.RelatedCharacterId, relation.Type, 0));
			}
		}

		/// <summary>
		/// Removes a character relation
		/// </summary>
		/// <param name="relCharId">The related character low <see cref="EntityId"/></param>
		/// <param name="relationType">The relation type</param>
		internal void RemoveRelation(uint charId, uint relCharId, CharacterRelationType relationType)
		{
			RelationResult relResult;

			var relation = GetRelation(charId, relCharId, relationType);

			if (RemoveRelation(relation))
			{
				relResult = GetDeleteRelationResult(relationType);
			}
			else
			{
				relResult = RelationResult.FRIEND_DB_ERROR;
				_log.Debug(Resources.CharacterRelationRemoveFailed, charId, relCharId, relationType, relResult);
			}

			//Send relation status to the client
			var remover = World.GetCharacter(charId);
			if (remover != null)
			{
				SendFriendStatus(remover, relCharId, string.Empty, relResult);
			}
		}

		/// <summary>
		/// Removes all Relations that the given Character has of the given type.
		/// </summary>
		/// <returns>Whether there were any Relations of the given type</returns>
		public bool RemoveRelations(uint charLowId, CharacterRelationType type)
		{
			bool success = false;

			m_lock.EnterWriteLock();
			try
			{
				success = m_activeRelations[(int)type].Remove(charLowId);

				success = success && m_passiveRelations[(int)type].Remove(charLowId);
			}
			finally
			{
				m_lock.ExitWriteLock();
			}

			return success;
		}

		/// <summary>
		/// Removes a character relation
		/// </summary>
		public bool RemoveRelation(IBaseRelation relation)
		{
			HashSet<IBaseRelation> relations;
			bool success = false;

			m_lock.EnterWriteLock();
			try
			{
				if (m_activeRelations[(int)relation.Type].TryGetValue(relation.CharacterId, out relations))
				{
					relations.Remove(relation);
				}
				if (m_passiveRelations[(int)relation.Type].TryGetValue(relation.RelatedCharacterId, out relations))
				{
					relations.Remove(relation);
					success = true;
				}
			}
			finally
			{
				m_lock.ExitWriteLock();
			}

			if (relation is PersistedRelation)
			{
				((PersistedRelation)relation).Delete();
			}

			_log.Debug(Resources.CharacterRelationRemoved, relation.CharacterId,
				relation.RelatedCharacterId, relation.Type, 0);

			return success;
		}

		/// <summary>
		/// Saves all relations of the Character with the given low EntityId
		/// </summary>
		public void SaveRelations(uint lowUid)
		{
			m_lock.EnterReadLock();

			try
			{
			    for (int i = 1; i < m_activeRelations.Length && (i != (int)CharacterRelationType.GroupInvite); i++)
				{
					var relations = m_activeRelations[i];
				    HashSet<IBaseRelation> relationList;
				    if (relations.TryGetValue(lowUid, out relationList))
					{
						foreach (var relation in relationList)
						{
							((PersistedRelation)relation).SaveToDB();
						}
					}
				}
			}
			finally
			{
				m_lock.ExitReadLock();
			}
		}


		public static BaseRelation CreateRelation(uint charId, uint relatedCharId,
			CharacterRelationType relationType)
		{
			switch (relationType)
			{
				case CharacterRelationType.Friend:
					return new FriendRelation(charId, relatedCharId);
				case CharacterRelationType.Ignored:
					return new IgnoredRelation(charId, relatedCharId);
				case CharacterRelationType.Muted:
					return new MutedRelation(charId, relatedCharId);
				case CharacterRelationType.GroupInvite:
					return new GroupInviteRelation(charId, relatedCharId);
				case CharacterRelationType.GuildInvite:
					return new GuildInviteRelation(charId, relatedCharId);
			}
			return null;
		}

		public static BaseRelation CreateRelation(CharacterRelationRecord relationRecord)
		{
			if (relationRecord == null)
				return null;

			return CreateRelation(relationRecord.CharacterId, relationRecord.RelatedCharacterId, relationRecord.RelationType);
		}

		public static void SendFriendStatus(Character target, uint friendId, string note, RelationResult relResult)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_FRIEND_STATUS))
			{
				packet.WriteByte((byte)relResult);

				switch (relResult)
				{
					case RelationResult.FRIEND_DB_ERROR:
					case RelationResult.FRIEND_NOT_FOUND:
					case RelationResult.IGNORE_NOT_FOUND:
					case RelationResult.MUTED_NOT_FOUND:
					case RelationResult.FRIEND_ENEMY:
					case RelationResult.FRIEND_SELF:
					case RelationResult.FRIEND_ALREADY:
					case RelationResult.FRIEND_REMOVED:
					case RelationResult.IGNORE_SELF:
					case RelationResult.IGNORE_ALREADY:
					case RelationResult.IGNORE_ADDED:
					case RelationResult.IGNORE_REMOVED:
					case RelationResult.MUTED_SELF:
					case RelationResult.MUTED_ALREADY:
					case RelationResult.MUTED_ADDED:
					case RelationResult.MUTED_REMOVED:
						packet.Write(EntityId.GetPlayerId(friendId));
						break;
					case RelationResult.FRIEND_ADDED_OFFLINE:
						packet.Write(EntityId.GetPlayerId(friendId));
						packet.WriteCString(note);
						break;
					case RelationResult.FRIEND_OFFLINE:
						packet.Write(EntityId.GetPlayerId(friendId));
						packet.WriteByte((byte)CharacterStatus.OFFLINE);
						break;
				}
				target.Client.Send(packet);
			}
		}

		public static void SendFriendOnline(Character target, Character friend, string note, bool justAdded)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_FRIEND_STATUS))
			{
				packet.WriteByte((byte)(justAdded ? RelationResult.FRIEND_ADDED_ONLINE : RelationResult.FRIEND_ONLINE));
				packet.Write(friend.EntityId);

                if (justAdded)
                    packet.WriteCString(note ?? string.Empty); //Friend Note

				packet.WriteByte((byte)friend.Status);
				packet.Write((int)friend.Zone.Id);
				packet.WriteUInt(friend.Level);
				packet.WriteUInt((byte)friend.Class);
				target.Client.Send(packet);
			}
		}

		/// <summary>
		/// Notifies all friends etc
		/// </summary>
		/// <param name="character">The character logging in</param>
		internal void OnCharacterLogin(Character character)
		{
			//Send the full relation list to the logging player
			SendRelationList(character, RelationTypeFlag.Friend | RelationTypeFlag.Ignore | RelationTypeFlag.Muted);

		    m_lock.EnterReadLock();
			try
			{
			    HashSet<IBaseRelation> relations;
			    if (m_passiveRelations[(int)CharacterRelationType.Friend].TryGetValue(character.EntityId.Low, out relations))
				{
					foreach (var relation in relations)
					{
						var related = World.GetCharacter(relation.CharacterId);
						if (related != null)
						{
							SendFriendOnline(related, character, relation.Note, false);
						}
					}
				}
			}
			finally
			{
				m_lock.ExitReadLock();
			}
		}

		/// <summary>
		/// Notifies all friends etc
		/// </summary>
		/// <param name="character">The character logging off</param>
		internal void OnCharacterLogout(Character character)
		{
			SaveRelations(character.EntityId.Low);

			//Notify related online players this character is logging off
			NotifyFriendRelations(character.EntityId.Low, RelationResult.FRIEND_OFFLINE);

			//Unload this character relations
			//RemoveRelations(character.EntityId.Low);
		}

		private void NotifyFriendRelations(uint friendLowId, RelationResult relResult)
		{
		    m_lock.EnterReadLock();
			try
			{
			    HashSet<IBaseRelation> relations;
			    if (m_passiveRelations[(int)CharacterRelationType.Friend].TryGetValue(friendLowId, out relations))
				{
					foreach (var relation in relations)
					{
						var target = World.GetCharacter(relation.CharacterId);
						if (target != null)
						{
							SendFriendStatus(target, friendLowId, relation.Note, relResult);
						}
					}
				}
			}
			finally
			{
				m_lock.ExitReadLock();
			}
		}

		[Initialization(InitializationPass.Fifth, "Start relation manager")]
		public static void StartRelationMgr()
		{
			Instance.Initialize();
		}

		#region Send/Reply methods

		/// <summary>
		/// Sends the specified character relation lists to the specified character
		/// </summary>
		/// <param name="character">The character to send the list</param>
		/// <param name="flag">Flag indicating which lists should be sent to the character</param>
		internal void SendRelationList(Character character, RelationTypeFlag flag)
		{
			var relations = GetFlatRelations(character.EntityId.Low, flag);

			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CONTACT_LIST))
			{
				//Write the requested list flag
				packet.WriteUInt((uint)flag);

				//Write the relations count
				packet.WriteUInt((uint)relations.Count);

				foreach (var entry in relations.Values)
				{
					//Write the related char Id
					packet.Write(EntityId.GetPlayerId(entry.RelatedCharacterId));

					//Write the contact flag
					packet.WriteUInt((uint)entry.Flag);

					//Write the contact note
					packet.WriteCString(entry.Note);

					//If this relation type requires online notification
					if (entry.Flag.HasFlag(RelationTypeFlag.Friend))
					{
						var relatedChar = World.GetCharacter(entry.RelatedCharacterId);
						if (relatedChar != null)
						{
							packet.WriteByte((byte)relatedChar.Status);
							packet.Write(relatedChar.Zone != null ? (int)relatedChar.Zone.Id : 0);
							packet.Write(relatedChar.Level);
							packet.Write((int)relatedChar.Class);
						}
						else
							packet.WriteByte((byte)CharacterStatus.OFFLINE);
					}
				}
				character.Client.Send(packet);
			}
		}

		private Dictionary<uint, RelationListEntry> GetFlatRelations(uint characterId, RelationTypeFlag flags)
		{
			var relations = new Dictionary<uint, RelationListEntry>();

			HashSet<IBaseRelation> friendRelations = GetRelations(characterId, CharacterRelationType.Friend);
			if (friendRelations != null)
			{
				foreach (IBaseRelation relation in friendRelations)
				{
				    relations[relation.RelatedCharacterId] = new RelationListEntry(relation, RelationTypeFlag.Friend);
				}
			}

			HashSet<IBaseRelation> ignoreRelations = GetRelations(characterId, CharacterRelationType.Ignored);
			if (ignoreRelations != null)
			{
				foreach (IBaseRelation relation in ignoreRelations)
				{
					if (relations.ContainsKey(relation.RelatedCharacterId))
					{
						relations[relation.RelatedCharacterId].Flag |= RelationTypeFlag.Ignore;
					}
					else
					{
					    relations[relation.RelatedCharacterId] = new RelationListEntry(relation, RelationTypeFlag.Ignore);
					}
				}
			}

			HashSet<IBaseRelation> mutedRelations = GetRelations(characterId, CharacterRelationType.Muted);
			if (mutedRelations != null)
			{
				foreach (IBaseRelation relation in mutedRelations)
				{
					if (relations.ContainsKey(relation.RelatedCharacterId))
					{
						relations[relation.RelatedCharacterId].Flag |= RelationTypeFlag.Muted;
					}
					else
					{
					    relations[relation.RelatedCharacterId] = new RelationListEntry(relation, RelationTypeFlag.Muted);
					}
				}
			}

			var removedKeys = from entry in relations
							  where !entry.Value.Flag.HasAnyFlag(flags)
                              select entry.Key;

		    foreach (uint key in removedKeys)
			{
				relations.Remove(key);
			}

			return relations;
		}

		private static RelationResult GetDeleteRelationResult(CharacterRelationType relationType)
		{
			switch (relationType)
			{
				case CharacterRelationType.Friend:
					return RelationResult.FRIEND_REMOVED;
				case CharacterRelationType.Ignored:
					return RelationResult.IGNORE_REMOVED;
				case CharacterRelationType.Muted:
					return RelationResult.MUTED_REMOVED;
			}
			throw new ArgumentOutOfRangeException("relationType");
		}

		#endregion

		private sealed class RelationListEntry
		{
			public uint RelatedCharacterId
			{
				get; 
                private set;
			}

			public RelationTypeFlag Flag
			{
				get;
				set;
			}

			private string _note = string.Empty;
			public string Note
			{
				get { return _note; }
			    private set
                {
                    _note = string.IsNullOrEmpty(value) ? string.Empty : value;
                }
			}

		    public RelationListEntry(IBaseRelation relation, RelationTypeFlag flag)
		    {
		        RelatedCharacterId = relation.RelatedCharacterId;
		        Flag = flag;
		        Note = relation.Note;
		    }
		}
	}
}