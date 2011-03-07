using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Factions;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Entities
{
	public interface ICharacterSet : IPacketReceiver
	{
		/// <summary>
		/// Amount of Characters in this set
		/// </summary>
		int CharacterCount { get; }

		/// <summary>
		/// FactionGroup or 0 if this is not a biased group
		/// </summary>
		FactionGroup FactionGroup { get; }

		/// <summary>
		/// Calls the given callback within each Character's context
		/// </summary>
		void ForeachCharacter(Action<Character> callback);

		/// <summary>
		/// Creates a Copy of the set
		/// </summary>
		Character[] GetAllCharacters();
	}

	public interface ICharacterCollection : ICharacterSet
	{
		void AddCharacter(Character chr);

		void RemoveCharacter(Character chr);
	}
}