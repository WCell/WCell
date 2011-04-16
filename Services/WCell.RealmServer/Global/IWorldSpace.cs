using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Global
{
	public interface IWorldSpace
	{
		IWorldSpace ParentSpace { get; }

		WorldStateCollection WorldStates { get; }

		void CallOnAllCharacters(Action<Character> action);
	}
}