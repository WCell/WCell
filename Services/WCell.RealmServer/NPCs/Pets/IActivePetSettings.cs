using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;

namespace WCell.RealmServer.NPCs.Pets
{
	/// <summary>
	/// Settings for an active pet to be saved to DB
	/// </summary>
	public interface IActivePetSettings
	{
		NPCId PetEntryId
		{
			get;
			set;
		}

		NPCEntry PetEntry
		{
			get;
		}

		int PetHealth
		{
			get;
			set;
		}

		int PetPower
		{
			get;
			set;
		}

		/// <summary>
		/// 
		/// </summary>
		SpellId PetSummonSpellId
		{
			get;
			set;
		}

		int PetDuration
		{
			get;
			set;
		}
	}
}
