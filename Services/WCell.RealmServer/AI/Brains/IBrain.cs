/*************************************************************************
 *
 *   file		: IBrain.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-02-11 01:52:08 +0800 (Wed, 11 Feb 2009) $

 *   revision		: $Rev: 750 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using WCell.Constants.Updates;
using WCell.Core.Timers;
using WCell.RealmServer.AI.Actions;
using WCell.RealmServer.AI.Groups;
using WCell.RealmServer.Entities;
using WCell.Util.Graphics;

namespace WCell.RealmServer.AI.Brains
{
	/// <summary>
	/// The interface to any brain (usually belonging to an NPC)
	/// A brain is a finite automaton with a queue of actions
	/// </summary>
	public interface IBrain : IUpdatable, IAICombatEventHandler, IDisposable
	{
		/// <summary>
		/// Current state of the brain
		/// </summary>
		BrainState State { get; set; }

		/// <summary>
		/// Default state of the brain
		/// </summary>
		BrainState DefaultState { get; set; }

		/// <summary>
		/// Aggressive brains actively seek for combat Action
		/// </summary>
		bool IsAggressive
		{
			get;
			set;
		}

		UpdatePriority UpdatePriority { get; }

		/// <summary>
		/// Current Running state
		/// </summary>
		/// <value>if false, Brain will not update</value>
		bool IsRunning { get; set; }

		/// <summary>
		/// The collection of all actions the IBrain can execute
		/// </summary>
		IAIActionCollection Actions { get; }

		/// <summary>
		/// The AIAction that is currently being executed
		/// </summary>
		IAIAction CurrentAction { get; set; }

		/// <summary>
		/// The origin location to which this Brain will always want to go back to (if any)
		/// </summary>
		Vector3 SourcePoint { get; set; }

		void EnterDefaultState();

		void StopCurrentAction();

		/// <summary>
		/// Executes a brain cycle
		/// </summary>
		void Perform();

		bool ScanAndAttack();

		bool CheckCombat();

		/// <summary>
		/// Used to get the owner of this brain out of combat and leave all fighting behind
		/// </summary>
		void ClearCombat(BrainState newState);

		/// <summary>
		/// Called when the AIGroup of an NPC is about to change
		/// </summary>
		void OnGroupChange(AIGroup newGroup);
	}
}