/*************************************************************************
 *
 *   file			: FactionCommands.cs
 *   copyright		: (C) The WCell Team
 *   email			: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-14 13:00:53 +0100 (to, 14 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1192 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{

	public class LoveAllCommand : RealmServerCommand
	{
		protected LoveAllCommand() { }

		protected override void Initialize()
		{
			Init("LoveAll");
			EnglishDescription = "Makes all factions fall in love with the Owner";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			((Character)trigger.Args.Target).Reputations.LoveAll();
			trigger.Reply("Everyone loves {0} now.", trigger.Args.Target);
		}

		public override ObjectTypeCustom TargetTypes
		{
			get
			{
				return ObjectTypeCustom.Player;
			}
		}
	}

	public class HateAllCommand : RealmServerCommand
	{
		protected HateAllCommand() { }

		protected override void Initialize()
		{
			Init("HateAll");
			EnglishDescription = "Makes all factions hate the Owner";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			((Character)trigger.Args.Target).Reputations.HateAll();
			trigger.Reply("Everyone dispises of {0} now.", trigger.Args.Target);
		}

		public override ObjectTypeCustom TargetTypes
		{
			get
			{
				return ObjectTypeCustom.Player;
			}
		}
	}
}