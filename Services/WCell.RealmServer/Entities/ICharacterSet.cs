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
		int Count { get; }

		FactionGroup FactionGroup { get; }

		void ForeachCharacter(Action<Character> callback);

		/// <summary>
		/// Creates a Copy of the set
		/// </summary>
		Character[] GetCharacters();
	}

	public interface ICharacterCollection : ICharacterSet
	{
		void AddCharacter(Character chr);

		void RemoveCharacter(Character chr);
	}
}