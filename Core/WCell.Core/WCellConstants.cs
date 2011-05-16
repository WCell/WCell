/*************************************************************************
 *
 *   file		: Constants.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-08-14 03:51:23 +0800 (Thu, 14 Aug 2008) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 591 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WCell.Constants;
using WCell.Util;
using System.Text;

namespace WCell.Core
{
	public static class WCellConstants
	{
		public static ClientLocale DefaultLocale = ClientLocale.English;

		public static Encoding DefaultEncoding = Encoding.UTF8;

		public const string AUTH_PATCH_DIR = "patch";

		public const string DBC_DIR = "dbc";

		public const string SCRIPT_DIR = "scripts";

		public const int SERVER_UPDATE_INTERVAL = 50;

		public const long MAX_NETWORK_SEND_DELTA = 50;

		public const int MAX_UNCOMPRESSED_UPDATE_PACKET = 128;

		public const int MAX_CLIENT_PACKET_SIZE = 128 << 10;

		public const int HEADER_CHANGE_THRESHOLD = 32767;

		/// <summary>
		/// The delay for every RealmServer to send updates to the AuthServer.
		/// If the AuthServer didn't receive an Update after this * 1.5, the Realm is considered offline.
		/// </summary>
		public static readonly TimeSpan RealmServerUpdateInterval = TimeSpan.FromMinutes(1);

		/// <summary>
		/// Root path of the server binaries
		/// </summary>
		public static string ROOT_PATH = Environment.CurrentDirectory;

		public static readonly uint ClassTypeLength = (uint)ClassId.End;
		public static readonly uint RaceTypeLength = (uint)RaceId.End;

		public static readonly ClassMask[] AllClassMasks = (ClassMask[])Enum.GetValues(typeof(ClassMask));
		public static readonly ClassId[] AllClassIds = (ClassId[])Enum.GetValues(typeof(ClassId));
		public static readonly RaceMask[] AllRaceMasks = (RaceMask[])Enum.GetValues(typeof(RaceMask));
		public static readonly RaceId[] AllRaceIds = (RaceId[])Enum.GetValues(typeof(RaceId));

		public static readonly Dictionary<ClassMask, ClassId> ClassTypesByMask = ((Func<Dictionary<ClassMask, ClassId>>)(() =>
		{
			var dict = new Dictionary<ClassMask, ClassId>();
			for (int i = 0; i < AllClassMasks.Length; i++)
			{
				if (Utility.GetSetIndices((uint)AllClassMasks[i]).Length == 1)
				{
					dict.Add(AllClassMasks[i], AllClassIds[i]);
				}
			}
			dict.Add(ClassMask.None, ClassId.PetTalents);
			return dict;
		}))();

		public static readonly Dictionary<RaceMask, RaceId> RaceTypesByMask = ((Func<Dictionary<RaceMask, RaceId>>)(() =>
		{
			var dict = new Dictionary<RaceMask, RaceId>();
			for (int i = 0; i < AllRaceMasks.Length; i++)
			{
				if (Utility.GetSetIndices((uint)AllRaceMasks[i]).Length == 1)
				{
					dict.Add(AllRaceMasks[i], AllRaceIds[i]);
				}
			}
			return dict;
		}))();

		public static RaceId GetRaceType(RaceMask mask)
		{
			RaceId id;
			if (!RaceTypesByMask.TryGetValue(mask, out id))
			{
				Debugger.Break();
			}
			return id;
		}
	}
}