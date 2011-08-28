/*************************************************************************
 *
 *   file		: AuraUID.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-03-07 14:58:12 +0800 (Sat, 07 Mar 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 784 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.Runtime.InteropServices;

namespace WCell.RealmServer.Spells.Auras
{
	/// <summary>
	/// Represents a unique Aura-identifier: 2 Auras are exactly the same, only
	/// if they have the same spell-id and are both either positive or negative.
	/// </summary>
	[StructLayout(LayoutKind.Explicit, Size = 4)]
	public struct AuraIndexId
	{
		public const uint AuraIdMask = 0x00FFFFFF;
		public static readonly AuraIndexId None;

		[FieldOffset(0)]
		public uint AuraUID;

		[FieldOffset(3)]
		public bool IsPositive;

		public AuraIndexId(uint auraUID, bool isPositive)
		{
			AuraUID = auraUID;
			IsPositive = isPositive;
		}

		public override bool Equals(object obj)
		{
			return obj is AuraIndexId && (((AuraIndexId)obj).AuraUID == AuraUID);
		}

		public override int GetHashCode()
		{
			return AuraUID.GetHashCode();
		}

		public override string ToString()
		{
			return (AuraUID & AuraIdMask) + (IsPositive ? " (Beneficial)" : " (Harmful)");
		}
	}
}