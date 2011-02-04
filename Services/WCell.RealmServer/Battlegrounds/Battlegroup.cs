using System.Collections.Generic;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.Battlegrounds
{
    /// <summary>
    /// A group of characters that are queued together
    /// TODO: Removal of characters
    /// TODO: Removal of offline characters
    /// TODO: Reference in characters, maybe instance collection?
    /// TODO: Delayed removal to allow for relogging, maybe instance collection?
    /// </summary>
    public class Battlegroup
    {
        private List<Character> m_characters;

        public Battlegroup(Character character)
        {
            m_characters = new List<Character>(1);
            m_characters.Add(character);
        }


        /// <summary>
        /// TODO: Checking for level requirements
        /// </summary>
        /// <param name="group"></param>
        public Battlegroup(Group group)
        {
            m_characters = new List<Character>(5);

            group.SyncRoot.EnterReadLock();
			try
			{
				foreach (var chr in group.GetAllCharacters())
				{
					if (chr != null)
					{
						m_characters.Add(chr);
						//BattlegroundHandler.SendAddedToQueue(chr.Client);
					}
				}
			}
			finally
			{
				group.SyncRoot.ExitReadLock();
			}
        }

        public int Size
        {
            get { return m_characters.Count; }
        }

        public void Delete()
        {
            foreach (var character in m_characters)
            {
                // BattlegroundHandler.SendRemovedFromQueue(character.Client);
            }

            m_characters.Clear();


            m_characters = null;
        }
    }
}