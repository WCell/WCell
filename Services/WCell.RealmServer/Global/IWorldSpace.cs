using System;
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