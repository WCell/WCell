/*************************************************************************
 *
 *   file		: QuestEnums.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-04-08 11:02:58 +0200 (út, 08 IV 2008) $
 *   last author	: $LastChangedBy: domiii $
 *   revision		: $Rev: 244 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;

namespace WCell.Constants.Quests
{
	public enum QuestType : uint
	{
		/// <summary>
		/// Group?
		/// </summary>
		Normal = 0,
		Elite = 1,
		Life = 21,
		PvP = 41,
		Raid = 62,
		Dungeon = 81,
		WorldEvent = 82,
		Legendary = 83,
		Escort = 84,
		Heroic = 85,
	}

	[Flags]
	public enum QuestSpecialFlags : uint
	{
		NoExtraRequirements = 0,
		MakeRepeateable = 1, // Makes the quest repeatable.
		EventCompletable = 2, // Makes the quest only completable by some external event (an entry in areatrigger_involvedrelation, spell effect quest complete or an entry in spell_scripts with command 7 as some examples)
		RepeateableEventCompleteable = 3 // Both repeatable and completable only through an external event 
	}

	public enum QuestStatus : byte
	{
		NotAvailable            = 0, // nothing really
		TooHighLevel            = 1, // Gray !
		Obsolete                = 2, // nothing, but there are obsolete quests available
		NotCompleted            = 5, // Gray ?
		RepeateableCompletable    = 6, // Yellow or blue ? need to try
		Repeatable              = 7, // Quest repeatable
		Available               = 8, // Yellow !
		CompletableNoMinimap      = 9, // Yellow ? - UNUSED
		Completable               = 10,  // Yellow ?
		Count
	}

	public static class QuestStatusHelper
	{
		public static bool CanStartOrFinish(this QuestStatus status)
		{
			return status >= QuestStatus.RepeateableCompletable;
		}
	}

	public enum QuestTemplateStatus : uint
	{
		Inactive = 0,    // 0 - Quest is not active and player can't do it.
		Session = 1,    // 1 - Can accept only in current session, will be deleted on start
		Active = 2     // 2 - Quest is active and player can take and do it.
	}

	public enum QuestCompleteStatus : byte
	{
		NotCompleted = 0,
		Completed = 1,
		Failed = 2
	}

	public enum QuestFailedReason : byte 
	{
		NoDetails       = 0,
		InventoryFull   = 4,
		DupeItemFound   = 17
	};

	public enum QuestInvalidReason : byte
	{
		Ok                  = 0xFF,
		NoRequirements      = 0,
		LowLevel            = 1,
		WrongClass          = 5,
		WrongRace           = 6,
		AlreadyCompleted    = 7,
		AlreadyOnTimedQuest = 12,
		AlreadyHave         = 13,
		NoExpansionAccount  = 16,
		NoRequiredItems     = 21,
		NotEnoughMoney      = 23,
		TooManyDailys       = 26,
		Tired             = 27 //"You cannot completed quests once you have reached tired time" - probably have something to do with parental control (and/or the Chinese client)
	};

	public enum QuestPushResponse : byte 
	{
		Sharing         = 0,
		CannotTake      = 1,
		AcceptedQuest   = 2,
		RefusedQuest    = 3,
		TooFar          = 4,
		Busy            = 5,
		QuestlogFull    = 6,
		AlreadyHave     = 7,
		AlreadyFinished = 8,
	};

	[Flags]
	public enum QuestFlags : uint
	{
		None          = 0x0000,
		Deliver       = 0x0001, // Stay Alive or else it's failed
		/// <summary>
		/// Escort and event based Quests
		/// </summary>
		Escort        = 0x0002,
		/// <summary>
		/// Explore Areas
		/// </summary>
		Explore       = 0x0004,
		/// <summary>
		/// Can be shared
		/// </summary>
		Sharable	  = 0x0008,
		Exploration   = 0x0010, //not used
		Timed         = 0x0020, // 7632
		Raid          = 0x0040,
		TBCOnly		  = 0x0080,
		DeliverMore   = 0x0100, // Quest needs more than normal quest item drops from mobs, maybe it's permanent blue question mark?
		HiddenRewards = 0x0200, //combined with 0x80
		Unknown4      = 0x0400,
		TBCRaces      = 0x0800,
		Daily         = 0x1000,
        AutoAccept    = 0x80000,                // quests in starting areas
	}
}