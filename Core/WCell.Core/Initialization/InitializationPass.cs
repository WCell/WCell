using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Core.Initialization
{
	public enum InitializationPass
	{
		/// <summary>
		/// During this step, the config will be loaded
		/// </summary>
		Config = 0,
        /// <summary>
        /// Initializes the database; connects. 
        /// Loads addons 
        /// Starts loading spells
        /// </summary>
		First,
        /// <summary>
        /// Initializes skills, factions
        /// Registers packet handlers
        /// Initializes Content
        /// Loads spell overrides
        /// </summary>
		Second ,
        /// <summary>
        /// Finalizes spells
        /// Initializes the world
        /// </summary>
		Third ,
        /// <summary>
        /// Loads DB content: NPCs, Items, AreaTriggers, WorldLocations, Commands, Taxi Pathes
        /// </summary>
		Fourth ,
        /// <summary>
        /// Initializes Auctions, Mail, EquipmentSetRecords, Guild Ids, Experience-Table, performance counters, ItemRecord, Repair Costs, Quests, GameObjects, Guilds, Instances
        /// Creates default chat channels
        /// </summary>
		Fifth ,
        /// <summary>
        /// Initialize Transports
        /// </summary>
		Sixth ,
        /// <summary>
        /// Initializes Races and Classes
        /// </summary>
		Seventh ,
        /// <summary>
        /// Initializes Battlegrounds
        /// </summary>
		Eighth ,
        /// <summary>
        /// Not used. Note the spelling mistake.
        /// </summary>
		Nineth ,
        /// <summary>
        /// Initializes Debug Tools
        /// Initializes Update Packets
        /// </summary>
		Tenth,
		Last,
		Any
	}
}