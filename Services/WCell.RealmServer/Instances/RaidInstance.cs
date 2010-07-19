using System;
using System.Collections.Generic;
using WCell.RealmServer.Global;
using WCell.Constants;

namespace WCell.RealmServer.Instances
{
    /* TODO: Saved Players
     * Saving of dead creatures
     * Loading of creatures
     */

    public abstract class RaidInstance : BaseInstance
	{
		private bool m_encounterActive;

		public readonly List<InstanceCollection> SavedPlayers = new List<InstanceCollection>();

		public RaidInstance()
		{
        }

        public bool EncounterActive
        {
			get { return m_encounterActive; }
				//Should keep players in combat and prevent players from entering the instance
            set
            {
            	if (m_encounterActive != value)
            	{
            		m_encounterActive = value;
            	}
            }
        }
    }
}