using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.Updates
{
	public class UpdateField
	{
		public const int ObjectTypeCount = (int)ObjectTypeId.AIGroup;

		public ObjectTypeId Group;
		public uint Offset;
		public uint Size;

		public UpdateFieldType Type;
		public UpdateFieldFlags Flags;
		public string Name;

		/// <summary>
		/// Indicates whether this UpdateField should be sent to everyone around (or only to the owner)
		/// </summary>
		public bool IsPublic;

		public string FullName
		{
			get
			{
				return Group + "Fields." + Name;
			}
		}

		public string FullTypeName
		{
			get
			{
				return "UpdateFieldType." + Type;
			}
		}

		public override string ToString()
		{
			return FullName + string.Format(" (Offset: {0}, Size: {1}, Type: {2}, Flags: {3})", Offset, Size, Type, Flags);
		}

		public override bool Equals(object obj)
		{
			if (obj is UpdateField)
			{
				var field2 = (UpdateField)obj;
				return field2.Group == Group && field2.Offset == Offset;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (int)Group | ((int)Offset << 3);	// guaranteed to be unique
		}
	}

	[Flags]
	public enum UpdateFieldFlags : uint
	{
		None = 0,
		/// <summary>
		/// Fields with this flag are to be known by all surrounding players
		/// </summary>
		Public = 0x1,
		/// <summary>
		/// Fields with this flag are only meant to be known by the player itself
		/// </summary>
		Private = 0x2,
		/// <summary>
		/// Fields with this flag are to be known by the owner, in the case of pets and a few item fields
		/// </summary>
		OwnerOnly = 0x4,
		/// <summary>
		/// Unused
		/// </summary>
		Flag_0x8_Unused = 0x8,
		/// <summary>
		/// ITEMSTACK_COUNT
		/// ITEMDURATION
		/// ITEMSPELL_CHARGES
		/// ITEMDURABILITY
		/// ITEMMAXDURABILITY
		/// </summary>
		ItemOwner = 0x10,
		/// <summary>
		/// _MINDAMAGE
		/// _MAXDAMAGE
		/// _MINOFFHANDDAMAGE
		/// _MAXOFFHANDDAMAGE
		/// _RESISTANCES
        /// These are public to the caster of the beast lore spell
		/// </summary>
		BeastLore = 0x20,
		/// <summary>
		/// Fields with this flag are only to be known by party members
		/// </summary>
		GroupOnly = 0x40,
		/// <summary>
		/// Unused
		/// </summary>
		Flag_0x80_Unused = 0x80,
		/// <summary>
		/// Differs from player to player
		/// In the case of health, it sends percents to everyone not in your party instead of the acutal value
		/// </summary>
		Dynamic = 0x100,
	}

	public enum UpdateFieldType
	{
		None = 0,
		UInt32 = 1,
		TwoInt16 = 2,
		Float = 3,
		Guid = 4,
		ByteArray = 5,
		Unk322 = 6
	}
}
